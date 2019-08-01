using UnityEngine;
using UnityEngine.UI;

namespace CDMZ.UI
{
    public static class UIComponents
    {
        private static Logger _logger = new Logger("CDMZ|UIComponents");
        public static GameObject ScrollablePrefab;
        public static GameObject ListEntry;

        public static void Init()
        {
            _logger.Info("Initializing...");
            //Build a mods ui
            var ui = Object.Instantiate(Find.TitleScreenUI.GetChildByName("ChallengeSelectionUI"));

            //Remove the challenge controller
            Object.DestroyImmediate(ui.GetComponent<ChallengeSelectionUI>());

            //Remove some elements we don't want
            Object.DestroyImmediate(ui.GetChildByName("CloneDroneChallengesButton"));
            Object.DestroyImmediate(ui.GetChildByName("WorkshopChallengesButton"));
            Object.DestroyImmediate(ui.GetChildByName("WorkshopChallengeScrollRoot"));
            Object.DestroyImmediate(ui.GetChildByName("GetMoreButton"));
            Object.DestroyImmediate(ui.GetChildByName("NoWorkshopChallengesUI"));
            Object.DestroyImmediate(ui.GetChildByName("LoadingSpinner"));

            ui.GetChildByName("twitchModeLabel (1)").name = "Title";
            ui.GetChildByName("NormalChallengeScrollRoot").name = "ScrollRoot";
            ui.name = "CDMZBasicScrollableWindow";

            ui.GetChildByName("Title").GetComponent<Text>().text = "CDMZ|ScrollableWindow";

            ScrollablePrefab = ui;

            var le = Object.Instantiate(GameUIRoot.Instance.TitleScreenUI.ChallengeSeclectionUI.DisplayPrefab.gameObject);
            
            Object.DestroyImmediate(le.GetComponent<ChallengeDisplay>());
            
            le.GetChildByName("challengeNameLabel").name = "TopText";
            le.GetChildByName("challengeImage").name = "Image";

            var panel = le.GetChildByName("RewardPanel");
            panel.name = "GrayPanel";
            panel.GetChildByName("rewardHeading").name = "GrayPanelRedText";
            panel.GetChildByName("rewardLabel").name = "GrayPanelGrayText";

            le.GetChildByName("completedLabel").name = "GrayPanelGreenText";
            le.GetChildByName("playButton").name = "RightButton";

            le.name = "CDMZListEntry";

            ListEntry = le;
        }
    }
}