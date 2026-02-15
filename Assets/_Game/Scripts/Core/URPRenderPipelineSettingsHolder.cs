using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using System.IO;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEditor;
#endif

namespace Core
{
    public static class URPRenderPipelineSettingsHolder
    {
        private static bool _assetInstantiated = false;

        [ShowInInspector, ReadOnly]
        private static UniversalRenderPipelineAsset _assetInstance = null;

        public static void Initialize()
        {
            var asset = GetSettingsAsset; // For clone

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnEditorPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private static void OnEditorPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode ||
                state == PlayModeStateChange.EnteredPlayMode)
            {
                UnityEditor.EditorApplication.playModeStateChanged -= OnEditorPlayModeStateChanged;

                RevertURPAssetChanges();
            }
        }

        private static void RevertURPAssetChanges()
        {
            if (_assetInstantiated)
            {
                _assetInstance = Resources.Load<UniversalRenderPipelineAsset>("URPMasterSettings");
                QualitySettings.renderPipeline = _assetInstance;
                GraphicsSettings.defaultRenderPipeline = _assetInstance;
                GraphicsSettings.defaultRenderPipeline = _assetInstance;
                string assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_assetInstance));
                string graphicsSettingsPath = "ProjectSettings/GraphicsSettings.asset";
                graphicsSettingsPath = Helpers.File.RelativePathToNormalPath(graphicsSettingsPath);
                if (File.Exists(graphicsSettingsPath))
                {
                    var allLines = File.ReadAllLines(graphicsSettingsPath).ToList();
                    for (int i = 0; i < allLines.Count; i++)
                    {
                        if (allLines[i].Contains("m_CustomRenderPipeline"))
                        {
                            if (allLines[i].Contains("type:"))
                            {
                                allLines[i] = "  m_CustomRenderPipeline: {fileID: 11400000, guid: " + assetGuid +
                                              ", type: 2}";
                            }
                            else
                            {
                                allLines[i] = "  m_CustomRenderPipeline: {fileID: 11400000, guid: " + assetGuid + ",";
                            }

                            //Helpers.File.WriteAllLinesWithoutAppendExtraLine(allLines, graphicsSettinsPath);
                            File.WriteAllLines(graphicsSettingsPath, allLines);
                            //Helpers.File.WriteAllLinesWithoutAppendExtraLine(allLines, graphicsSettinsPath);
                            return;
                        }
                    }
                }

                _assetInstantiated = false;
            }
        }
#endif

        public static UniversalRenderPipelineAsset GetSettingsAsset
        {
            get
            {
                if (_assetInstance == null)
                {
                    _assetInstance = Resources.Load<UniversalRenderPipelineAsset>($"URPMasterSettings");
#if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
                        _assetInstance = UnityEngine.Object.Instantiate(_assetInstance);
                        QualitySettings.renderPipeline = _assetInstance;
                        GraphicsSettings.defaultRenderPipeline = _assetInstance;
                        GraphicsSettings.defaultRenderPipeline = _assetInstance;

                        var originalRendererDataList = _assetInstance.rendererDataList;
                        var newRendererDataList = new ScriptableRendererData[originalRendererDataList.Length];
                        for (int i = 0; i < originalRendererDataList.Length; i++)
                        {
                            if (originalRendererDataList[i] != null)
                            {
                                newRendererDataList[i] = UnityEngine.Object.Instantiate(originalRendererDataList[i]);
                            }
                        }

                        typeof(UniversalRenderPipelineAsset)
                            .GetField("m_RendererDataList",
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.SetValue(_assetInstance, newRendererDataList);

                        _assetInstantiated = true;
                    }
#endif
                }

                return _assetInstance;
            }
        }
    }
}