using System.Collections;
using System.Collections.Generic;
using System.IO;
using SpektraGames.RuntimeUI.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SpektraGames.RuntimeUI.Editor
{
    [InitializeOnLoad]
    public static class RuntimeUIEditorInitializer
    {
        static RuntimeUIEditorInitializer()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;

            if (!RuntimeUISettings.Instance)
            {
                string mainResourcesFolder = "Assets/Resources";
                if (!AssetDatabase.IsValidFolder(mainResourcesFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                    AssetDatabase.Refresh();
                }

                string soName = nameof(RuntimeUISettings);
                string soPath = Path.Combine(mainResourcesFolder, soName + ".asset");
                RuntimeUISettings so = ScriptableObject.CreateInstance<RuntimeUISettings>();
                so.ToastBehaviourReference = Resources.Load<GameObject>("RuntimeUI/Base_ToastBehaviour").GetComponent<ToastBehaviour>();
                so.DynamicPopupBehaviourReference = Resources.Load<GameObject>("RuntimeUI/Base_DynamicPopupBehaviour").GetComponent<DynamicPopupBehaviour>();
                so.FullScreenLoadingBehaviourReference = Resources.Load<GameObject>("RuntimeUI/Base_FullScreenLoading").GetComponent<FullScreenLoadingBehaviour>();
                AssetDatabase.CreateAsset(so, soPath);
                AssetDatabase.Refresh();
            }
        }
    }
}