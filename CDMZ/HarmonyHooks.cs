using System;
using System.Collections.Generic;
using CDMZ.EventSystem;
using Harmony;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CDMZ
{
    public static class HarmonyHooks
    {
        /**
         * Contains a list of all the characters currently existing. We use this over CharacterTracker because that
         * only contains ATTACHED characters, not all of them.
         */
        private static List<Character> allKnownCharacters = new List<Character>();

        public static void Init()
        {
            ModdingZoneHooks.ModZoneLogger.Info("HarmonyHooks Patching...");
            
            //Character patches.
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(Character), "Awake"), new HarmonyMethod(typeof(HarmonyHooks), nameof(OnCharacterAwake)));
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(Character), "OnDestroy"), new HarmonyMethod(typeof(HarmonyHooks), nameof(OnCharacterDestroy)));
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

        public static void OnCharacterAwake(Character __instance)
        {
            ModdingZoneHooks.ModZoneLogger.Debug($"Character Awakening: {__instance}");

            if (!EventBus.Instance.Post(new CharacterSpawnEvent(__instance)))
            {
                ModdingZoneHooks.ModZoneLogger.Debug($"Preventing spawn of character {__instance}");
                Object.Destroy(__instance);
                return;
            }
            
            allKnownCharacters.Add(__instance);
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
    }
}