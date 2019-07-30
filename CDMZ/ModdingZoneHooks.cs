using System;
using System.Reflection;
using CDMZ.EventSystem;
using Harmony;
using JetBrains.Annotations;

namespace CDMZ
{
    [UsedImplicitly]
    public static class ModdingZoneHooks
    {
        internal static readonly Logger ModZoneLogger = new Logger("CDMZ");
        internal static readonly Logger VanillaLogger = new Logger("Game");

        internal static HarmonyInstance Harmony;

        private static bool _init;
        
        [UsedImplicitly]
        public static void InitializeAll()
        {
            try
            {
                if (!_init)
                {
                    VanillaLogger.Info("Game Starting!");
                    //Called from the very first line of SplashScreenFlowManager.Start - literally the line of code that is executed first as far as I can tell.

                    //First things first, let's get harmony set up.
                    Harmony = HarmonyInstance.Create("samboy.clonedronemoddingzone_harmonyinst");
                    Harmony.PatchAll(Assembly.GetExecutingAssembly());

                    HarmonyHooks.DoInitialPatches();

                    _init = true;
                }
            }
            catch (Exception e)
            {
                ModZoneLogger.Exception(e, "Error during initialization");
            }
        }
    }
}