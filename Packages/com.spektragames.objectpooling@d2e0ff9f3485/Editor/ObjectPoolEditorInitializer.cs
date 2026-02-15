using System.Collections;
using System.Collections.Generic;
using System.IO;
using SpektraGames.ObjectPooling.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SpektraGames.ObjectPooling.Editor
{
    [InitializeOnLoad]
    public static class ObjectPoolEditorInitializer
    {
        static ObjectPoolEditorInitializer()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;

            if (PoolContainer.Instance == null)
            {
                string mainResourcesFolder = "Assets/Resources";
                if (!AssetDatabase.IsValidFolder(mainResourcesFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                    AssetDatabase.Refresh();
                }

                string soName = nameof(PoolContainer);
                string soPath = Path.Combine(mainResourcesFolder, soName + ".asset");
                ScriptableObject so = ScriptableObject.CreateInstance<PoolContainer>();
                AssetDatabase.CreateAsset(so, soPath);
                AssetDatabase.Refresh();
            }
        }
    }
}