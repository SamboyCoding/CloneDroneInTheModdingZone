using System;
using System.Linq;
using System.Threading;
using CDMZ.EventSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CDMZ
{
    public class CDMZSplashScreenManager : MonoBehaviour
    {
        private Logger _logger = new Logger("CDMZ|Splash");
        private Text modText;

        private static bool _finishedLoading;
        private static bool _shouldGoToNextScene;
        private static bool _hasTriggeredSceneLoad;

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
            modText.text = "CDMZ : Initializing";
            modText.alignment = TextAnchor.UpperLeft;

            _logger.Debug($"Injected splash screen at pos {transform1.localPosition}. Size {modText.preferredWidth}x{modText.preferredHeight}: {modText}");
            
            SceneManager.GetActiveScene().DumpHierarchy(_logger);

            var launchThread = new Thread(() =>
            {
                try
                {
                    HarmonyHooks.DoOnLoadPatches();

                    modText.text = "CDMZ: Setting up event bus";

                    new EventBus();

                    EventListener.Init(); //TODO: Make event bus detect all on creation.

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