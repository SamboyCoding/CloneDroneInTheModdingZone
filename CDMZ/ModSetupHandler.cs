using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CDMZ.EventSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CDMZ
{
    public class ModSetupHandler : MonoBehaviour
    {
        private Logger _logger = new Logger("CDMZ|Splash");
        private Text modText;

        private static bool _finishedLoading;
        private static bool _shouldGoToNextScene;
        private static bool _hasTriggeredSceneLoad;

        private string _modDirectoryPath = Path.Combine(Path.Combine(Application.dataPath, ".."), "mods");

        private void Awake()
        {
            _logger.Info("Splash! Manager injected successfully.");

            var uiRoot = SceneManager.GetActiveScene().GetRootGameObjects().First(o => o.name.Contains("UIRoot"));
            
            modText = Instantiate(SplashScreenFlowManager.Instance.SaveDataLabel, uiRoot.transform, true);

            var canvasRect = uiRoot.GetComponent<Canvas>().pixelRect;
            
            _logger.Debug($"Splash screen dimensions are {canvasRect.width} by {canvasRect.height}");
            
            var transform1 = modText.transform;
            transform1.localPosition =  new Vector3(-30, 280);
            modText.gameObject.SetActive(true);
            modText.text = "CDMZ : Running Init Patches";
            modText.alignment = TextAnchor.UpperLeft;

            _logger.Debug($"Injected splash screen label at pos {transform1.localPosition}. Size {modText.preferredWidth}x{modText.preferredHeight}: {modText}");
            
            //SceneManager.GetActiveScene().DumpHierarchy(_logger);

            var launchThread = new Thread(() =>
            {
                try
                {
                    HarmonyHooks.DoOnLoadPatches();

                    if (!Directory.Exists(_modDirectoryPath))
                        Directory.CreateDirectory(_modDirectoryPath);

                    modText.text = "CDMZ: Discovering mods";

                    foreach (var file in Directory.GetFiles(_modDirectoryPath))
                    {
                        if (!file.EndsWith(".dll")) continue;
                        
                        _logger.Debug($"Loading assembly {file}");
                        try
                        {
                            var asm = Assembly.LoadFile(file);
                            ReflectionHelper.ModAssemblies.Add(asm);
                        }
                        catch (Exception e)
                        {
                            _logger.Exception(e, $"Failed to load assembly {file}");
                        }
                    }

                    modText.text = "CDMZ: Constructing mods";
                    
                    ModManager.ConstructAll();

                    modText.text = "CDMZ: Loading ModBot mods in compatibility mode";

                    ModManager.LoadModBotMods();

                    modText.text = "CDMZ: Setting up event bus";

                    new EventBus();

                    modText.text = "CDMZ: Enabling mods";

                    ModManager.ExecuteEnableForEnabledMods();

                    modText.text = "CDMZ: Done";

                    _finishedLoading = true;
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "Caught exception during launch");
                }
            }) {IsBackground = true};

            launchThread.Start();
        }

        private void Update()
        {
            if (_finishedLoading && _shouldGoToNextScene && !_hasTriggeredSceneLoad)
            {
                _logger.Debug($"Loading scene {SplashScreenFlowManager.Instance.GameplaySceneName}");
                SceneManager.LoadSceneAsync(SplashScreenFlowManager.Instance.GameplaySceneName);
                _hasTriggeredSceneLoad = true;
            }
        }

        public static bool GoToNextSceneReplacement()
        {
            _shouldGoToNextScene = true;

            return false;
        }

        
    }
}