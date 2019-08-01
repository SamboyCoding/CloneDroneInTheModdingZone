using UnityEngine;
using UnityEngine.UI;

namespace CDMZ.UI
{
    public class ModsListUI : MonoBehaviour
    {
        public static ModsListUI Instance;
        
        private Logger _logger = new Logger("CDMZ|ModsList");
        private GameObject _listItemPrefab = UIComponents.ListEntry;

        private Transform _displayHolder;

        public ModsListUI()
        {
            _logger.Debug("Injected into main UI");
            gameObject.GetChildByName("Title").GetComponent<Text>().text = "Mods";

            var button = gameObject.GetChildByName("exitButton").GetComponent<Button>(); 
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(OnCloseClick);

            _displayHolder = gameObject.GetChildByName("ScrollRoot").GetChildByName("Viewport").GetChildByName("DisplayHolder").transform;
            
            Instance = this;
        }

        public void OnCloseClick()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            PopulateList();
        }

        private void PopulateList()
        {
            TransformUtils.DestroyAllChildren(_displayHolder);
            
            foreach (var mod in ModManager.Mods)
            {
                var entry = Instantiate(_listItemPrefab, _displayHolder, false);

                entry.GetChildByName("TopText").GetComponent<Text>().text = mod.GetModName();
                entry.GetChildByName("descriptionLabel").GetComponent<Text>().text = mod.GetModDescription();

                entry.GetChildByName("RightButton").GetChildByName("heading").GetComponent<Text>().text = "Disable";
                entry.GetChildByName("RightButton").GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                entry.GetChildByName("RightButton").GetComponent<Button>().onClick.AddListener(() => OnModButtonClick(mod));

                if (mod.IsUpToDate())
                {
                    entry.GetChildByName("GrayPanel").SetActive(false);
                    entry.GetChildByName("GrayPanelGreenText").SetActive(true);
                    entry.GetChildByName("GrayPanelGreenText").GetComponent<Text>().text = $"v{mod.GetVersion()}";
                }
                else
                {
                    entry.GetChildByName("GrayPanelGreenText").SetActive(false);
                    entry.GetChildByName("GrayPanel").SetActive(true);
                    entry.GetChildByName("GrayPanel").GetChildByName("GrayPanelRedText").GetComponent<Text>().text = $"Latest: v{mod.GetLatestVersion()}";
                    entry.GetChildByName("GrayPanel").GetChildByName("GrayPanelGrayText").GetComponent<Text>().text = $"Current: v{mod.GetVersion()}";
                }
            }
        }

        private void OnModButtonClick(Mod mod)
        {
            
        }
    }
}