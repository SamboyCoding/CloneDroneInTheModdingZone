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

        private static bool _init = false;
        
        [UsedImplicitly]
        public static void InitializeAll()
        {
            try
            {
                HarmonyHooks.GameFlowManagerPostFix.startTimeTicks = DateTime.Now.Ticks;
                
                if (!_init)
                {
                    VanillaLogger.Info("Game Starting!");
                    //Called from the very first line of GameFlowManager.Start - literally the line of code that is executed first as far as I can tell.

                    ModZoneLogger.Info("Initializing Harmony... Target assembly is " + Assembly.GetExecutingAssembly());

                    //First things first, let's get harmony set up.
                    Harmony = HarmonyInstance.Create("samboy.clonedronemoddingzone_harmonyinst");
                    Harmony.PatchAll(Assembly.GetExecutingAssembly());

                    HarmonyHooks.Init();

                    ModZoneLogger.Info("Harmony Setup Complete. Initializing event handlers...");

                    GameFlowManager.Instance.gameObject.AddComponent<EventBus>();

                    EventListener.Init();
                    //TODO Mod event listeners

                    ModZoneLogger.Info("Finished event handler init.");

                    _init = true;
                }

                EventBus.Instance.Post(new MainMenuShownEvent());
            }
            catch (Exception e)
            {
                ModZoneLogger.Exception(e, "Error during initialization");
            }
        }
    }
}