using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CDMZ
{
    public static class Extensions
    {
        public static string Repeat(this string toRepeat, int count)
        {
            var ret = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                ret.Append(toRepeat);
            }

            return ret.ToString();
        }

        public static void DumpHierarchy(this Scene scene, Logger dumpTo)
        {
            dumpTo.Debug("All Loaded Objects:\n\n");
            foreach (var o in scene.GetRootGameObjects())
            {
                DumpRecursive(o);
            }
            
            dumpTo.Debug("---End Dump---\n\n");
            
            void DumpRecursive(GameObject gameObject, int indentLevel = 0)
            {
                dumpTo.Debug($"{"    ".Repeat(indentLevel)}{gameObject} ({gameObject.transform.childCount} children). Pos: Local {gameObject.transform.localPosition} / Global {gameObject.transform.position}.");
                foreach (Transform child in gameObject.transform)
                {
                    DumpRecursive(child.gameObject, indentLevel + 1);
                }
            }
        }
    }
}