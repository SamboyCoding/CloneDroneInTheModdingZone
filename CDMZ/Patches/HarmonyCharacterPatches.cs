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

        public static void OnCharacterDie(Character __instance)
        {
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
            
            return EventBus.Instance.Post(new PreDamageEvent(__instance, bodyPart));
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