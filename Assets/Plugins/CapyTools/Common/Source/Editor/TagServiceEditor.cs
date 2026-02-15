using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CapyTools.Common.Editor
{
    [CustomEditor(typeof(TagService))]
    public class TagServiceEditor : UnityEditor.Editor
    {
        private static TagService instance;
        /// <summary>
        /// Ensures that the TagService asset exists in Resources.
        /// </summary>
        /// 
        [UnityEditor.Callbacks.DidReloadScripts, UnityEditor.Callbacks.RunAfterAssembly("CapyTools.Common")]
        static void EnsureTagServiceExists()
        {
            EditorApplication.delayCall += () =>
            {
                instance = Resources.Load<TagService>("TagService");

                if (instance == null)
                {
                    instance = ScriptableObject.CreateInstance<TagService>();

                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                        Debug.Log("TagService: Resources folder created in Assets.");
                    }

                    AssetDatabase.CreateAsset(instance, "Assets/Resources/TagService.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Debug.Log("TagService.asset created in Resources folder.");
                }

                FetchTags();
            };
        }

        /// <summary>
        /// Fetches tags from Unity and updates TagService.
        /// </summary>
        private static void FetchTags()
        {
            string[] unityTags = UnityEditorInternal.InternalEditorUtility.tags;

            if (instance != null && !AreTagsEqual(unityTags))
            {
                instance.SetTags(unityTags);
                EditorUtility.SetDirty(instance);
                Debug.Log("TagService: tags updated.");
            }
        }

        /// <summary>
        /// Checks if the tags stored in TagService are the same as Unity's current tags.
        /// </summary>
        private static bool AreTagsEqual(string[] unityTags)
        {
            if (instance == null || instance.Tags == null || instance.Tags.Length != unityTags.Length)
                return false;

            for (int i = 0; i < unityTags.Length; i++)
            {
                if (instance.Tags[i] != unityTags[i])
                    return false;
            }

            return true;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("TagService holds all the tags in the project and allows access to them at runtime." +
                " This tags array is updated automatically when assemblies are reloaded. Use TagService.GetTags() to retrieve the tags.", MessageType.Info);
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            if (GUILayout.Button("Update Tags"))
            {
                FetchTags();
            }
        }

    }
}
