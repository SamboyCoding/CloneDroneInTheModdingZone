using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CDMZ.EventSystem;
using Harmony;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CDMZ
{
    public static class HarmonyHooks
    {
        private static Logger _logger = new Logger("CDMZ|Harmony");
        /**
         * Contains a list of all the characters currently existing. We use this over CharacterTracker because that
         * only contains ATTACHED characters, not all of them.
         */
        private static List<Character> allKnownCharacters = new List<Character>();

        public static void Init()
        {
            ModdingZoneHooks.ModZoneLogger.Info("HarmonyHooks Patching...");

            //Character patches.
            
            //Patch to disable spawn
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(EnemyFactory), nameof(EnemyFactory.SpawnEnemyWithRotation), new []{typeof(Transform), typeof(Vector3), typeof(Vector3), typeof(CharacterModel)}), prefix: new HarmonyMethod(typeof(HarmonyHooks), nameof(OnCharacterSpawn)));
            
            //TODO: Patch to add to the char list
            
            //Patch to fix non null safe code
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method("<SpawnCurrentLevel>c__Iterator2:MoveNext"), transpiler: new HarmonyMethod(typeof(HarmonyHooks), nameof(LevelManagerSpawnCancelPatch)));
            
            //Patch to broadcast destruction
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(Character), "OnDestroy"), new HarmonyMethod(typeof(HarmonyHooks), nameof(OnCharacterDestroy)));
            
            //Patch to potentially disable death
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(Character), "onDeath"), new HarmonyMethod(typeof(HarmonyHooks), nameof(OnCharacterDie)));
        }


        [HarmonyPatch(typeof(GameFlowManager))]
        [HarmonyPatch("ShowTitleScreen")]
        [UsedImplicitly]
        public class GameFlowManagerPostFix
        {
            public static long startTimeTicks;

            [UsedImplicitly]
            public static void Postfix()
            {
                var span = new TimeSpan(DateTime.Now.Ticks - startTimeTicks);
                ModdingZoneHooks.VanillaLogger.Info("Main game flow initialized and title screen shown in " + span.TotalMilliseconds + " milliseconds.");
            }
        }

        public static bool OnCharacterSpawn(Transform enemyPrefab, Vector3 spawnPosition, Vector3 spawnRotation, CharacterModel characterModelOverride)
        {
            //Try to find enemy type
            var config = EnemyFactory.Instance.Enemies.First(c => c.EnemyPrefab == enemyPrefab);
            
            ModdingZoneHooks.ModZoneLogger.Debug($"Spawning a character of type {config.EnemyType}");

            if (!EventBus.Instance.Post(new CharacterSpawnEvent(config.EnemyType)))
            {
                ModdingZoneHooks.ModZoneLogger.Debug($"Preventing spawn of character of type {config.EnemyType}");
                return false;
            }

            return true;
        }

        public static void OnCharacterDestroy(Character __instance)
        {
            ModdingZoneHooks.ModZoneLogger.Debug($"Removing destroyed character {__instance}");
            allKnownCharacters.Remove(__instance);
        }

        public static void OnCharacterDie(Character __instance)
        {
            ModdingZoneHooks.ModZoneLogger.Debug($"{__instance} died.");
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
                        _logger.Debug($"Yield1 {instruction}");
                        yield return instruction;
                    }
                    else if (foundSpawnEnemy && !amended && instruction.opcode == OpCodes.Stloc_S)
                    {
                        _logger.Debug(instruction.operand.GetType().ToString());
                        amended = true;
                        _logger.Debug("Patching LevelManager's SpawnCurrentLevel to allow nulls from SpawnEnemy");

                        _logger.Debug($"Yield2 {instruction}");
                        yield return instruction; //We still have to store the enemy as a local.

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
                        _logger.Debug($"Yield2 {ret}");
                        yield return ret;

                        //If it's null jump over the four instructions
                        ret = new CodeInstruction(OpCodes.Brfalse, label);
                        _logger.Debug($"Yield2 {ret}");
                        yield return ret;

                        //Otherwise we have to once more load the 

                        _logger.Debug($"Yield2 {first}");
                        yield return first;

                        _logger.Debug($"Yield2 {second}");
                        yield return second;

                        _logger.Debug($"Yield2 {third}");
                        yield return third;

                        _logger.Debug($"Yield2 {fourth}");
                        yield return fourth;

                        _logger.Debug($"Yield2 {target}");
                        yield return target;
                    }
                    else
                    {
                        _logger.Debug($"Yield3 {instruction}");
                        yield return instruction;
                    }
                    
                    if(pos >= instructions.Count - 1)
                        yield break;

                    enumerator.MoveNext();
                    pos++;
                }
            }
        }
    }
}