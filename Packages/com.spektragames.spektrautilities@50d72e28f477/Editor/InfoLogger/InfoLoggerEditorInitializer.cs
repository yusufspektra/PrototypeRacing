using System.IO;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEditor;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Editor
{
    [InitializeOnLoad]
    public static class InfoLoggerEditorInitializer
    {
        static InfoLoggerEditorInitializer()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;

            if (!InfoLoggerSettings.Instance)
            {
                string mainResourcesFolder = "Assets/Resources";
                if (!AssetDatabase.IsValidFolder(mainResourcesFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                    AssetDatabase.Refresh();
                }

                string soName = nameof(InfoLoggerSettings);
                string soPath = Path.Combine(mainResourcesFolder, soName + ".asset");
                InfoLoggerSettings so = ScriptableObject.CreateInstance<InfoLoggerSettings>();
                AssetDatabase.CreateAsset(so, soPath);
                AssetDatabase.Refresh();
            }
        }
    }
}