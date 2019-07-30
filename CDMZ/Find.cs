using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CDMZ
{
    public static class Find
    {
        public static GameObject UIRoot => SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(o => o.name == "UIRoot");

        public static GameObject TitleScreenUI => UIRoot.GetChildByName("TitleScreenUI");

        public static GameObject TitleScreenButtonList => TitleScreenUI.GetChildByName("ButtonsBG").GetChildByName("BG");
    }
}