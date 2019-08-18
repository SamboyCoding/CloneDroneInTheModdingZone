using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CDMZ.EventSystem;
using Harmony;
using UnityEngine;

namespace CDMZ.Patches
{
    public static class HarmonyCharacterPatches
    {
        /**
         * Contains a list of all the characters currently existing. We use this over CharacterTracker because that
         * only contains ATTACHED characters, not all of them.
         */
        private static List<Character> allKnownCharacters = new List<Character>();

        public static bool PreCharacterSpawn(Transform enemyPrefab, Vector3 spawnPosition, Vector3 spawnRotation, CharacterModel characterModelOverride)
        {
            //Try to find enemy type
            var config = EnemyFactory.Instance.Enemies.First(c => c.EnemyPrefab == enemyPrefab);

            ModdingZoneHooks.ModZoneLogger.Debug($"Spawning a character of type {config.EnemyType}");

            if (!EventBus.Instance.Post(new CharacterPreSpawnEvent(config.EnemyType)))
            {
                ModdingZoneHooks.ModZoneLogger.Debug($"Preventing spawn of character of type {config.EnemyType}");
                return false;
            }

            return true;
        }

        public static void PostCharacterSpawn(Character __instance)
        {
            EventBus.Instance.Post(new CharacterPostSpawnEvent(__instance));
            allKnownCharacters.Add(__instance);
        }

        public static void OnCharacterDestroy(Character __instance)
        {
            ModdingZoneHooks.ModZoneLogger.Debug($"Removing destroyed character {__instance}");
            allKnownCharacters.Remove(__instance);

            EventBus.Instance.Post(new CharacterDespawnEvent(__instance));
        }

        public static void OnFPMDie(FirstPersonMover __instance)
        {
            ModdingZoneHooks.ModZoneLogger.Debug($"{__instance} died.");

            //TODO Future - see if we can transpile FPM#dropWeaponsOrKillPlayerFromDamagedBodyPart
            EventBus.Instance.Post(new CharacterDeathEvent(__instance));
        }

        public static void OnCharacterDie(Character __instance)
        {
            if (__instance is FirstPersonMover) return;

            ModdingZoneHooks.ModZoneLogger.Debug($"{__instance} died.");
            EventBus.Instance.Post(new CharacterDeathEvent(__instance));
        }

        public static bool OnPreDamage(MeleeImpactArea __instance, MechBodyPart bodyPart)
        {
            //TODO: this doesn't include spidertron ("MortarWalker") grenade fragments ("BulletProjectile"s)

            //Observations:
            //Spear comes up as sword
            //Fire doesn't come up at all
            //Player-fired arrows have no owner

            return EventBus.Instance.Post(new CharacterPreDamageEvent(__instance, bodyPart));
        }

        public static IEnumerable<CodeInstruction> LevelEnemySpawnerSpawnCancelPatch(IEnumerable<CodeInstruction> i, ILGenerator generator)
        {
            //Need to look for two things here, because there's two calls to EnemyFactory.

            var instructions = i.ToList();
            var foundFirst = false;
            var foundSecond = false;

            var patchedFirst = false;
            var patchedSecond = false;
            using (var enumerator = instructions.GetEnumerator())
            {
                enumerator.MoveNext();
                CodeInstruction instruction;
                var pos = 0;
                while ((instruction = enumerator.Current) != null)
                {
                    if (instruction.opcode == OpCodes.Callvirt
                        && instruction.operand is MethodInfo info
                        && info.DeclaringType == typeof(EnemyFactory)
                        && info.Name == nameof(EnemyFactory.SpawnEnemy))
                    {
                        if (!foundFirst)
                            foundFirst = true;
                        else if (!foundSecond)
                            foundSecond = true;
                        yield return instruction;
                    }
                    else if (foundFirst && !patchedFirst || foundSecond && !patchedSecond)
                    {
                        //As of 0.14.1.40, on first call at least, we call the spawnenemy method
                        //And then the following three instructions bump the return value into the enemyModel field and then reload that onto the stack (ldarg.0 followed by ldfld to get a this._enemyModel read)
                        //So we want to preserve those three

                        var instructionStoreField = instruction;
                        
                        enumerator.MoveNext();
                        pos++;
                        var instructionLdThis = enumerator.Current;
                        
                        enumerator.MoveNext();
                        pos++;
                        if (patchedFirst)
                        {
                            //Skip the second ldarg.0 on the second patch
                            enumerator.MoveNext();
                            pos++;
                        }
                        var instructionLdFld = enumerator.Current;

                        yield return instructionStoreField;
                        yield return instructionLdThis;
                        yield return instructionLdFld;

                        enumerator.MoveNext();
                        pos++;
                        var instructionContinueOriginalMethodBody = enumerator.Current;
                        var label = generator.DefineLabel();

                        //Now we check for null

                        //Easiest way - if NOT null jump to the rest of the original method
                        yield return new CodeInstruction(OpCodes.Brtrue, label);
                        //Otherwise, return. Simple, right?
                        yield return new CodeInstruction(OpCodes.Ret);

                        //Unfortunately, the "rest of the original method" is expecting us to have the model on the stack.

                        //Alternatively, if we're talking the second set, we're actually expecting "this", THEN the model.
                        if (patchedFirst)
                        {
                            //I.e. we're patching #2 now
                            var instructionFirstLoadThis = new CodeInstruction(OpCodes.Ldarg_0);
                            instructionFirstLoadThis.labels.Add(label); //This is where we jump to if non-null
                            yield return instructionFirstLoadThis;
                        }


                        //So push "this" back onto the stack...
                        var instructionReloadThis = new CodeInstruction(OpCodes.Ldarg_0);
                        if(!patchedFirst)
                            instructionReloadThis.labels.Add(label); //And this is where we want to go if non-null, but only for first patch

                        //...And reload the original field
                        var instructionReloadField = new CodeInstruction(OpCodes.Ldfld, instructionLdFld.operand);

                        yield return instructionReloadThis;
                        yield return instructionReloadField;

                        //Then continue with what should have happened.
                        yield return instructionContinueOriginalMethodBody;

                        if (!patchedFirst)
                            patchedFirst = true;
                        else
                            patchedSecond = true;
                    }
                    else
                    {
                        yield return instruction;
                    }
                    
                    if (pos >= instructions.Count - 1)
                        yield break;

                    enumerator.MoveNext();
                    pos++;
                }
            }
        }

        public static IEnumerable<CodeInstruction> LevelManagerSpawnCancelPatch(IEnumerable<CodeInstruction> i, ILGenerator generator)
        {
            var instructions = i.ToList();
            var foundSpawnEnemy = false;
            var amended = false;
            using (var enumerator = instructions.GetEnumerator())
            {
                enumerator.MoveNext();
                CodeInstruction instruction;
                var pos = 0;
                while ((instruction = enumerator.Current) != null)
                {
                    if (instruction.opcode == OpCodes.Callvirt && !foundSpawnEnemy
                                                               && instruction.operand is MethodInfo info
                                                               && info.DeclaringType == typeof(EnemyFactory)
                                                               && info.Name == nameof(EnemyFactory.SpawnEnemy))
                    {
                        foundSpawnEnemy = true;
                        yield return instruction;
                    }
                    else if (foundSpawnEnemy && !amended && instruction.opcode == OpCodes.Stloc_S)
                    {
                        amended = true;
                        HarmonyHooks.Logger.Debug("Patching LevelManager's SpawnCurrentLevel to allow nulls from SpawnEnemy");

                        yield return instruction; //We still have to store the enemy as a local variable for later use.

                        enumerator.MoveNext();
                        var first = enumerator.Current;
                        pos++;

                        enumerator.MoveNext();
                        var second = enumerator.Current;
                        pos++;

                        enumerator.MoveNext();
                        var third = enumerator.Current;
                        pos++;

                        enumerator.MoveNext();
                        var fourth = enumerator.Current;
                        pos++;

                        enumerator.MoveNext();
                        var target = enumerator.Current;
                        pos++;

                        var label = generator.DefineLabel();
                        target.labels.Add(label);

                        //Load the enemy back onto the stack
                        var ret = new CodeInstruction(OpCodes.Ldloc_S, instruction.operand);
                        yield return ret;

                        //If it's null jump over the four instructions
                        ret = new CodeInstruction(OpCodes.Brfalse, label);
                        yield return ret;

                        //Otherwise execute them - and actually handle the spawn
                        yield return first;
                        yield return second;
                        yield return third;
                        yield return fourth;

                        //And remember to include the original target instruction 
                        yield return target;
                    }
                    else
                    {
                        yield return instruction;
                    }

                    if (pos >= instructions.Count - 1)
                        yield break;

                    enumerator.MoveNext();
                    pos++;
                }
            }
        }
    }
}