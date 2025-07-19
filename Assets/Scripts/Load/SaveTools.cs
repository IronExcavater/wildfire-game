using System.IO;
using UnityEditor;
using UnityEngine;

namespace Load
{
    public static class SaveTools
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "Save/");

        [MenuItem("Tools/Save/Open Save Folder")]
        public static void OpenSaveFolder()
        {
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            EditorUtility.RevealInFinder(SavePath);
            Debug.Log($"Opened Save Folder: {SavePath}");
        }

        [MenuItem("Tools/Save/Reset Save Data")]
        public static void ResetSaveData()
        {
            if (Directory.Exists(SavePath))
            {
                Directory.Delete(SavePath, true);
                Debug.Log("✓ Save data deleted");
            }
            else Debug.Log("⚠ No save data found");

            Directory.CreateDirectory(SavePath);
        }
    }
}
