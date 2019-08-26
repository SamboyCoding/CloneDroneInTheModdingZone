using System.Linq;
using CDMZ.EventSystem;
using CDMZ.Patches;
using CDMZ.UI;
using Harmony;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CDMZ
{
    public static class HarmonyHooks
    {
        internal static Logger Logger = new Logger("CDMZ|Harmony");

        public static void DoInitialPatches()
        {
            ModdingZoneHooks.ModZoneLogger.Info("Injecting splash manager...");

            //Splash Screen Patches
            //When this is first executed NOTHING is set up. No SplashScreenFlowManager, no nothing.
            //So we patch Update() and on first frame we set stuff up, and we'll let that handle everything.

            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(SplashScreenFlowManager), "Update"), new HarmonyMethod(typeof(HarmonyHooks), nameof(InitialSplashScreenSetup)));

            //Make sure the splash screen doesn't end before we want it to.
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(SplashScreenFlowManager), nameof(SplashScreenFlowManager.GoToNextScene)), new HarmonyMethod(typeof(ModSetupHandler), nameof(ModSetupHandler.GoToNextSceneReplacement)));
        }

        public static void DoOnLoadPatches()
        {
            #region Character Patches

            //Patch to allow disabling spawn
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(EnemyFactory), nameof(EnemyFactory.SpawnEnemyWithRotation), new[] {typeof(Transform), typeof(Vector3), typeof(Vector3), typeof(CharacterModel)}), new HarmonyMethod(typeof(HarmonyCharacterPatches), nameof(HarmonyCharacterPatches.PreCharacterSpawn)));
            //Patch for when a character spawns
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(Character), "Awake"), new HarmonyMethod(typeof(HarmonyCharacterPatches), nameof(HarmonyCharacterPatches.PostCharacterSpawn)));
            //Patches to fix non null safe code
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method("<SpawnCurrentLevel>c__Iterator2:MoveNext"), transpiler: new HarmonyMethod(typeof(HarmonyCharacterPatches), nameof(HarmonyCharacterPatches.LevelManagerSpawnCancelPatch)));
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(LevelEnemySpawner), "Start"), transpiler: new HarmonyMethod(typeof(HarmonyCharacterPatches), nameof(HarmonyCharacterPatches.LevelEnemySpawnerSpawnCancelPatch)));
            //Patch for when a character('s ragdoll) is removed
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(Character), "OnDestroy"), new HarmonyMethod(typeof(HarmonyCharacterPatches), nameof(HarmonyCharacterPatches.OnCharacterDestroy)));
            //Patches to potentially disable death
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(FirstPersonMover), "onDeath"), new HarmonyMethod(typeof(HarmonyCharacterPatches), nameof(HarmonyCharacterPatches.OnFPMDie)));
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(Character), "onDeath"), new HarmonyMethod(typeof(HarmonyCharacterPatches), nameof(HarmonyCharacterPatches.OnCharacterDie)));

            //Damage patches
            foreach (var method in
                from subclass in ReflectionHelper.AllSubclassesOf(typeof(MeleeImpactArea))
                let method = AccessTools.Method(subclass, "tryDamageBodyPart")
                where method.DeclaringType == subclass
                select method)
            {
                ModdingZoneHooks.Harmony.Patch(method, new HarmonyMethod(typeof(HarmonyCharacterPatches), nameof(HarmonyCharacterPatches.OnPreDamage)));
            }
            #endregion
            
            //Level loading 
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(LevelManager), nameof(LevelManager.SpawnCurrentLevel)), new HarmonyMethod(typeof(HarmonyHooks), nameof(OnSpawnCurrentLevel)));
            ModdingZoneHooks.Harmony.Patch(AccessTools.Method(typeof(LevelManager), "CreateLevelTransformFromLevelEditorData"), new HarmonyMethod(typeof(HarmonyHooks), nameof(OnCreateLevelTransform)));
        }

        public static void OnSpawnCurrentLevel(LevelManager __instance)
        {
            var ld = __instance.GetCurrentLevelDescription();
            
            //These cases are picked up by the patch to CreateLevelTransformFromLevelEditorData as it's better.
            if (ld.LevelTags.Contains(LevelTags.LevelEditor) || ld.IsStreamedMultiplayerLevel || ld.IsPlayfabHostedLevel || ld.IsLevelEditorLevel()) return;
            
            //If we're here we're on a legacy level
            EventBus.Instance.Post(new LevelAboutToLoadEvent(ld.PrefabName));
        }

        public static void OnCreateLevelTransform(LevelManager __instance)
        {
            EventBus.Instance.Post(new LevelAboutToLoadEvent(__instance.GetCurrentLevelDescription()));
        }


        [HarmonyPatch(typeof(GameFlowManager))]
        [HarmonyPatch("ShowTitleScreen")]
        [UsedImplicitly]
        public class GameFlowManagerPostFix
        {
            [UsedImplicitly]
            public static void Postfix()
            {
                UIComponents.Init();
                
                Logger.Info("Injecting mods list UI...");
                
                //Set up the mods list and inject into UI
                var modsList = Object.Instantiate(UIComponents.ScrollablePrefab, Find.TitleScreenUI.transform, false);
                modsList.AddComponent<ModsListUI>(); //Set up the singleton
                
                EventBus.Instance.Post(new MainMenuShownEvent());
                //SceneManager.GetActiveScene().DumpHierarchy(new Logger("Scene Dump"));

                GameFlowManager.Instance.gameObject.AddComponent<Logger.QuitDetector>();

                Find.TitleScreenUI.GetChildByName("VersionLabel").GetComponent<Text>().text += "\nModded with CDMZ\nBy Samboy063";
                Find.TitleScreenUI.GetChildByName("VersionLabel").GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;

                Find.TitleScreenButtonList.GetChildByName("OptionsButton").MoveDown(20);
                Find.TitleScreenButtonList.GetChildByName("LevelEditorButton").MoveDown(20);
                Find.TitleScreenButtonList.GetChildByName("CreditsButton").MoveDown(20);
                Find.TitleScreenButtonList.GetChildByName("QuitButton").MoveDown(20);

                var modsButton = Object.Instantiate(Find.TitleScreenButtonList.GetChildByName("OptionsButton"), Find.TitleScreenButtonList.transform, true);
                modsButton.transform.localPosition = new Vector3(modsButton.transform.localPosition.x, Find.TitleScreenButtonList.GetChildByName("OptionsButton").transform.localPosition.y + 32);
                modsButton.GetComponentInChildren<Text>().text = "Mods";
                modsButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                modsButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Logger.Debug("Mods button clicked");
                    ModsListUI.Instance.gameObject.SetActive(true);
                });
            }
        }

        private static bool _splashSetup;

        public static void InitialSplashScreenSetup()
        {
            if (_splashSetup) return;
            
            _splashSetup = true;
            SplashScreenFlowManager.Instance.gameObject.AddComponent<ModSetupHandler>();
        }

        public static bool DisableMethod()
        {
            return false;
        }


        
    }
}