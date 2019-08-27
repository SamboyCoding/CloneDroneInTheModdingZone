using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ModLibrary
{
    /*
     * 
     * NOTE: the reason why this class isnt called "Debug" is becuase there is already a class called that in UnityEngine and that would cause a 
     * lot problems for people making mods (most people will use both this namespace and UnityEngine)
     * 
    */
    
    /*
     * Another Note: This class is heavily modified from the one in ModBot because we don't use the in-game console it does.
     */

    /// <summary>
    /// Allows you to write to the in-game console (open it with F1).
    /// </summary>
    public static class debug
    {
        /// <summary>
        /// Writes to the in-game console.
        /// </summary>
        /// <param name="_log"></param>
        public static void Log(string _log)
        {
            Debug.Log(_log);
        }


        public static void Log(object _log)
        {
            Debug.Log(_log);
        }

        /// <summary>
        /// Writes to the in-game console, in color.
        /// </summary>
        /// <param name="_log"></param>
        /// <param name="_color"></param>
        public static void Log(string _log, Color _color)
        {
            Debug.Log(_log);
        }

        /// <summary>
        /// Passes every instance of the given list's 'ToString()' value to: 'debug.Log()'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void PrintAll<T>(List<T> list)
        {
            foreach (var entry in list)
            {
                Debug.Log(entry.ToString());
            }
        }

        /// <summary>
        /// Passes every instance of the given list's 'ToString()' value to: 'debug.Log()'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void PrintAll<T>(List<T> list, Color _color)
        {
            foreach (var entry in list)
            {
                Debug.Log(entry.ToString());
            }
        }

        public static void PrintAllChildren(Transform obj)
        {
            outputText = "";
            WriteToFile(obj.ToString());
            RecursivePrintAllChildren("   ", obj);

            File.WriteAllText(Application.persistentDataPath + "/debug.txt", outputText);
            Process.Start("notepad.exe", Application.persistentDataPath + "/debug.txt");
        }

        private static void RecursivePrintAllChildren(string pre, Transform obj)
        {
            Component[] components = obj.gameObject.GetComponents(typeof(Component));

            if (components.Length != 0)
            {
                WriteToFile(pre + "Components: ");
            }

            for (int i = 0; i < components.Length; i++)
            {
                WriteToFile(pre + components[i].ToString());
            }

            if (obj.childCount != 0)
            {
                WriteToFile(pre + "Children: ");
            }

            for (int i = 0; i < obj.childCount; i++)
            {
                Transform child = obj.GetChild(i);
                WriteToFile(pre + i + ": " + child.name);
                RecursivePrintAllChildren(pre + "   ", child);
            }
        }

        private static void WriteToFile(string msg)
        {
            outputText += msg + "\n";
        }

        private static string outputText;
    }
}
