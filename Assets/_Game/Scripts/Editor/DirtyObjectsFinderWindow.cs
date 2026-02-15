#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils.Editor
{
    /// <summary>
    /// Editor window that displays all dirty (modified) GameObjects and components
    /// across all loaded scenes. Only works in Editor mode.
    /// </summary>
    public class DirtyObjectsFinderWindow : EditorWindow
    {
        #region Data Classes

        private class DirtyPropertyInfo
        {
            public string PropertyPath;
            public string DisplayName;
            public string Value;
            public string OriginalValue;
            public bool IsPrefabOverride;
        }

        private class DirtyComponentInfo
        {
            public Component Component;
            public string TypeName;
            public bool IsExpanded;
            public List<DirtyPropertyInfo> DirtyProperties = new List<DirtyPropertyInfo>();
            public bool IsPrefabInstance;
        }

        private class DirtyGameObjectInfo
        {
            public GameObject GameObject;
            public bool IsGameObjectDirty;
            public List<DirtyComponentInfo> DirtyComponents = new List<DirtyComponentInfo>();
            public bool IsExpanded;
            public List<DirtyPropertyInfo> GameObjectDirtyProperties = new List<DirtyPropertyInfo>();
            public bool IsPrefabInstance;

            public int TotalDirtyCount => (IsGameObjectDirty ? 1 : 0) + DirtyComponents.Count;
        }

        private class DirtySceneInfo
        {
            public Scene Scene;
            public string SceneName;
            public List<DirtyGameObjectInfo> DirtyGameObjects = new List<DirtyGameObjectInfo>();
            public bool IsExpanded = true;

            public int TotalDirtyObjectCount => DirtyGameObjects.Count;
            public int TotalDirtyComponentCount => DirtyGameObjects.Sum(go => go.DirtyComponents.Count);
        }

        #endregion

        #region Private Fields

        private List<DirtySceneInfo> _dirtyScenes = new List<DirtySceneInfo>();
        private Vector2 _scrollPosition;
        private string _searchFilter = "";
        private int _selectedSceneIndex = 0;
        private string[] _sceneFilterOptions;
        private bool _autoRefresh = false;
        private double _lastRefreshTime;
        private const double AUTO_REFRESH_INTERVAL = 1.0;

        // Statistics
        private int _totalDirtyScenes;
        private int _totalDirtyObjects;
        private int _totalDirtyComponents;

        // Styles
        private GUIStyle _headerStyle;
        private GUIStyle _foldoutStyle;
        private GUIStyle _statsStyle;
        private GUIStyle _warningBoxStyle;
        private GUIStyle _searchFieldStyle;
        private GUIStyle _itemButtonStyle;
        private GUIStyle _componentLabelStyle;
        private GUIStyle _sceneLabelStyle;
        private GUIStyle _propertyNameStyle;
        private GUIStyle _propertyValueStyle;
        private GUIStyle _prefabBadgeStyle;
        private bool _stylesInitialized;

        // Colors
        private static readonly Color SceneHeaderColor = new Color(0.3f, 0.5f, 0.7f, 0.3f);
        private static readonly Color GameObjectHeaderColor = new Color(0.4f, 0.6f, 0.4f, 0.2f);
        private static readonly Color ComponentItemColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
        private static readonly Color PropertyItemColor = new Color(0.6f, 0.4f, 0.6f, 0.15f);
        private static readonly Color PrefabOverrideColor = new Color(0.2f, 0.6f, 0.9f, 0.2f);
        private static readonly Color WarningColor = new Color(1f, 0.8f, 0.2f, 0.3f);

        // Icons
        private static Texture2D _gameObjectIcon;
        private static Texture2D _componentIcon;
        private static Texture2D _sceneIcon;
        private static Texture2D _refreshIcon;
        private static Texture2D _saveIcon;
        private static Texture2D _pingIcon;
        private static Texture2D _propertyIcon;
        private static Texture2D _prefabIcon;

        #endregion

        #region Menu Item & Window Setup

        [MenuItem("Tools/Dirty Objects Finder")]
        public static void OpenWindow()
        {
            var window = GetWindow<DirtyObjectsFinderWindow>();
            window.titleContent = new GUIContent("Dirty Objects Finder", EditorGUIUtility.IconContent("d_SceneViewFx").image);
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            LoadIcons();
            RefreshDirtyObjects();
            EditorApplication.update += OnEditorUpdate;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneClosed;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
        }

        private void OnSceneSaved(Scene scene)
        {
            RefreshDirtyObjects();
            Repaint();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            RefreshDirtyObjects();
            Repaint();
        }

        private void OnSceneClosed(Scene scene)
        {
            RefreshDirtyObjects();
            Repaint();
        }

        private void OnEditorUpdate()
        {
            if (_autoRefresh && !Application.isPlaying)
            {
                if (EditorApplication.timeSinceStartup - _lastRefreshTime > AUTO_REFRESH_INTERVAL)
                {
                    RefreshDirtyObjects();
                    Repaint();
                }
            }
        }

        private void LoadIcons()
        {
            _gameObjectIcon = EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D;
            _componentIcon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
            _sceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
            _refreshIcon = EditorGUIUtility.IconContent("d_Refresh").image as Texture2D;
            _saveIcon = EditorGUIUtility.IconContent("SaveAs").image as Texture2D;
            _pingIcon = EditorGUIUtility.IconContent("d_ViewToolZoom").image as Texture2D;
            _propertyIcon = EditorGUIUtility.IconContent("d_editicon.sml").image as Texture2D;
            _prefabIcon = EditorGUIUtility.IconContent("d_Prefab Icon").image as Texture2D;
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(5, 5, 5, 5)
            };

            _foldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };

            _statsStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 5, 5)
            };

            _warningBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 10, 10)
            };

            _searchFieldStyle = new GUIStyle(EditorStyles.toolbarSearchField)
            {
                fixedHeight = 20
            };

            _itemButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(5, 5, 2, 2)
            };

            _componentLabelStyle = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(20, 5, 2, 2)
            };

            _sceneLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12
            };

            _propertyNameStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Italic,
                fontSize = 10
            };

            _propertyValueStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                wordWrap = true
            };

            _prefabBadgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.4f, 0.7f, 1f) },
                fontStyle = FontStyle.Bold,
                fontSize = 9
            };

            _stylesInitialized = true;
        }

        #endregion

        #region Core Logic

        private void RefreshDirtyObjects()
        {
            _lastRefreshTime = EditorApplication.timeSinceStartup;
            _dirtyScenes.Clear();

            if (Application.isPlaying) return;

            // Get all loaded scenes
            int sceneCount = EditorSceneManager.sceneCount;
            List<string> sceneNames = new List<string> { "All Scenes" };

            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = EditorSceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                sceneNames.Add(scene.name);

                // Only scan scenes that Unity considers dirty (shows '*' in hierarchy)
                if (!scene.isDirty) continue;

                DirtySceneInfo sceneInfo = new DirtySceneInfo
                {
                    Scene = scene,
                    SceneName = scene.name
                };

                // Get all root GameObjects in the scene
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject root in rootObjects)
                {
                    ScanGameObjectRecursive(root, sceneInfo);
                }

                if (sceneInfo.DirtyGameObjects.Count > 0)
                {
                    _dirtyScenes.Add(sceneInfo);
                }
            }

            _sceneFilterOptions = sceneNames.ToArray();

            // Update statistics
            _totalDirtyScenes = _dirtyScenes.Count;
            _totalDirtyObjects = _dirtyScenes.Sum(s => s.TotalDirtyObjectCount);
            _totalDirtyComponents = _dirtyScenes.Sum(s => s.TotalDirtyComponentCount);
        }

        private void ScanGameObjectRecursive(GameObject gameObject, DirtySceneInfo sceneInfo)
        {
            DirtyGameObjectInfo goInfo = null;
            bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(gameObject);

            // Check if the GameObject itself is dirty
            if (EditorUtility.IsDirty(gameObject))
            {
                goInfo = new DirtyGameObjectInfo
                {
                    GameObject = gameObject,
                    IsGameObjectDirty = true,
                    IsPrefabInstance = isPrefabInstance
                };

                // Collect dirty properties for the GameObject
                goInfo.GameObjectDirtyProperties = CollectDirtyProperties(gameObject, isPrefabInstance);
            }

            // Check all components on this GameObject
            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component == null) continue;

                if (EditorUtility.IsDirty(component))
                {
                    if (goInfo == null)
                    {
                        goInfo = new DirtyGameObjectInfo
                        {
                            GameObject = gameObject,
                            IsGameObjectDirty = false,
                            IsPrefabInstance = isPrefabInstance
                        };
                    }

                    var compInfo = new DirtyComponentInfo
                    {
                        Component = component,
                        TypeName = component.GetType().Name,
                        IsPrefabInstance = isPrefabInstance
                    };

                    // Collect dirty properties for this component
                    compInfo.DirtyProperties = CollectDirtyProperties(component, isPrefabInstance);

                    goInfo.DirtyComponents.Add(compInfo);
                }
            }

            if (goInfo != null)
            {
                sceneInfo.DirtyGameObjects.Add(goInfo);
            }

            // Recursively scan children
            foreach (Transform child in gameObject.transform)
            {
                ScanGameObjectRecursive(child.gameObject, sceneInfo);
            }
        }

        private List<DirtyPropertyInfo> CollectDirtyProperties(UnityEngine.Object obj, bool isPrefabInstance)
        {
            var dirtyProperties = new List<DirtyPropertyInfo>();

            try
            {
                if (isPrefabInstance)
                {
                    // For prefab instances, get property modifications (overrides)
                    CollectPrefabOverrides(obj, dirtyProperties);
                }
                else
                {
                    // For non-prefab objects, collect modified serialized properties
                    CollectModifiedSerializedProperties(obj, dirtyProperties);
                }
            }
            catch (Exception)
            {
                // Silently handle any errors during property collection
            }

            return dirtyProperties;
        }

        private void CollectPrefabOverrides(UnityEngine.Object obj, List<DirtyPropertyInfo> dirtyProperties)
        {
            GameObject rootPrefab = null;

            if (obj is GameObject go)
            {
                rootPrefab = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            }
            else if (obj is Component comp)
            {
                rootPrefab = PrefabUtility.GetOutermostPrefabInstanceRoot(comp.gameObject);
            }

            if (rootPrefab == null) return;

            PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(rootPrefab);
            if (modifications == null) return;

            foreach (var mod in modifications)
            {
                if (mod.target == obj)
                {
                    // Skip internal/system properties
                    if (ShouldSkipProperty(mod.propertyPath)) continue;

                    dirtyProperties.Add(new DirtyPropertyInfo
                    {
                        PropertyPath = mod.propertyPath,
                        DisplayName = GetDisplayName(mod.propertyPath),
                        Value = FormatPropertyValue(mod.value),
                        OriginalValue = GetOriginalPrefabValue(obj, mod.propertyPath),
                        IsPrefabOverride = true
                    });
                }
            }
        }

        private void CollectModifiedSerializedProperties(UnityEngine.Object obj, List<DirtyPropertyInfo> dirtyProperties)
        {
            SerializedObject serializedObject = new SerializedObject(obj);
            SerializedProperty iterator = serializedObject.GetIterator();

            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                // Skip internal properties
                if (ShouldSkipProperty(iterator.propertyPath)) continue;

                // Only include properties that appear to have non-default values
                if (HasNonDefaultValue(iterator))
                {
                    dirtyProperties.Add(new DirtyPropertyInfo
                    {
                        PropertyPath = iterator.propertyPath,
                        DisplayName = iterator.displayName,
                        Value = GetSerializedPropertyValueString(iterator),
                        OriginalValue = "(default)",
                        IsPrefabOverride = false
                    });

                    // Limit to prevent UI overload
                    if (dirtyProperties.Count >= 50) break;
                }
            }

            serializedObject.Dispose();
        }

        private bool ShouldSkipProperty(string propertyPath)
        {
            // Skip Unity's internal properties
            string[] skipPatterns =
            {
                "m_ObjectHideFlags",
                "m_CorrespondingSourceObject",
                "m_PrefabInstance",
                "m_PrefabAsset",
                "m_GameObject",
                "m_Script",
                "m_Enabled",
                "m_EditorHideFlags",
                "m_EditorClassIdentifier",
                "m_Father",
                "m_Children",
                "m_LocalRotation",
                "m_LocalPosition",
                "m_LocalScale"
            };

            foreach (var pattern in skipPatterns)
            {
                if (propertyPath.StartsWith(pattern)) return true;
            }

            return false;
        }

        private string GetDisplayName(string propertyPath)
        {
            // Convert property path to a more readable display name
            string name = propertyPath;

            // Remove array indices formatting
            if (name.Contains(".Array.data["))
            {
                name = System.Text.RegularExpressions.Regex.Replace(name, @"\.Array\.data\[(\d+)\]", "[$1]");
            }

            // Remove m_ prefix
            if (name.StartsWith("m_"))
            {
                name = name.Substring(2);
            }

            // Add spaces before capitals
            name = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");

            return name;
        }

        private string FormatPropertyValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "(empty)";
            if (value.Length > 100) return value.Substring(0, 97) + "...";
            return value;
        }

        private string GetOriginalPrefabValue(UnityEngine.Object obj, string propertyPath)
        {
            try
            {
                UnityEngine.Object sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                if (sourcePrefab != null)
                {
                    SerializedObject so = new SerializedObject(sourcePrefab);
                    SerializedProperty prop = so.FindProperty(propertyPath);
                    if (prop != null)
                    {
                        string value = GetSerializedPropertyValueString(prop);
                        so.Dispose();
                        return value;
                    }
                    so.Dispose();
                }
            }
            catch (Exception)
            {
                // Ignore errors
            }

            return "(unknown)";
        }

        private bool HasNonDefaultValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue != 0;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return Math.Abs(property.floatValue) > 0.0001f;
                case SerializedPropertyType.String:
                    return !string.IsNullOrEmpty(property.stringValue);
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue != null;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex != 0;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value != Vector2.zero;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value != Vector3.zero;
                case SerializedPropertyType.Vector4:
                    return property.vector4Value != Vector4.zero;
                case SerializedPropertyType.Color:
                    return property.colorValue != Color.clear && property.colorValue != Color.white;
                case SerializedPropertyType.ArraySize:
                    return property.intValue > 0;
                default:
                    return true; // For complex types, assume they have value
            }
        }

        private string GetSerializedPropertyValueString(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString();
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString("F3");
                case SerializedPropertyType.String:
                    string str = property.stringValue;
                    if (str.Length > 50) str = str.Substring(0, 47) + "...";
                    return $"\"{str}\"";
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue != null
                        ? property.objectReferenceValue.name
                        : "(None)";
                case SerializedPropertyType.Enum:
                    return property.enumDisplayNames.Length > property.enumValueIndex && property.enumValueIndex >= 0
                        ? property.enumDisplayNames[property.enumValueIndex]
                        : property.enumValueIndex.ToString();
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString("F2");
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString("F2");
                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString("F2");
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString();
                case SerializedPropertyType.Rect:
                    return property.rectValue.ToString();
                case SerializedPropertyType.Bounds:
                    return property.boundsValue.ToString();
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue.eulerAngles.ToString("F1");
                case SerializedPropertyType.ArraySize:
                    return $"Size: {property.intValue}";
                case SerializedPropertyType.Generic:
                    return "(Complex)";
                default:
                    return $"({property.propertyType})";
            }
        }

        private string GetGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        private bool MatchesFilter(string name)
        {
            if (string.IsNullOrEmpty(_searchFilter)) return true;
            return name.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion

        #region GUI Drawing

        private void OnGUI()
        {
            InitializeStyles();

            DrawToolbar();

            if (Application.isPlaying)
            {
                DrawPlayModeWarning();
                return;
            }

            DrawStatistics();
            DrawSearchBar();
            DrawDirtyObjectsList();
            DrawFooter();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                // Refresh button
                if (GUILayout.Button(new GUIContent(" Refresh", _refreshIcon, "Scan all scenes for dirty objects"),
                        EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    RefreshDirtyObjects();
                }

                GUILayout.Space(5);

                // Auto-refresh toggle
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                _autoRefresh = GUILayout.Toggle(_autoRefresh, new GUIContent(" Auto", "Automatically refresh every second"),
                    EditorStyles.toolbarButton, GUILayout.Width(50));
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(10);

                // Scene filter dropdown
                EditorGUILayout.LabelField("Scene:", GUILayout.Width(45));
                if (_sceneFilterOptions != null && _sceneFilterOptions.Length > 0)
                {
                    _selectedSceneIndex = EditorGUILayout.Popup(_selectedSceneIndex, _sceneFilterOptions,
                        EditorStyles.toolbarPopup, GUILayout.Width(150));
                }

                GUILayout.FlexibleSpace();

                // Status indicator
                Color originalColor = GUI.backgroundColor;
                if (_totalDirtyObjects > 0)
                {
                    GUI.backgroundColor = new Color(1f, 0.6f, 0.2f);
                }
                else
                {
                    GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
                }

                string statusText = _totalDirtyObjects > 0
                    ? $" {_totalDirtyObjects} Dirty"
                    : " Clean";
                GUILayout.Label(new GUIContent(statusText), EditorStyles.toolbarButton, GUILayout.Width(70));
                GUI.backgroundColor = originalColor;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPlayModeWarning()
        {
            GUILayout.Space(20);

            EditorGUILayout.BeginVertical();
            {
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    Color originalBg = GUI.backgroundColor;
                    GUI.backgroundColor = WarningColor;

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300), GUILayout.Height(80));
                    {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("Play Mode Active", _headerStyle);
                        EditorGUILayout.LabelField("Dirty object tracking is disabled during play mode.",
                            EditorStyles.wordWrappedLabel);
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndVertical();

                    GUI.backgroundColor = originalBg;

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawStatistics()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                GUILayout.FlexibleSpace();

                // Scenes stat
                DrawStatBox("Scenes", _totalDirtyScenes.ToString(), _sceneIcon);
                GUILayout.Space(20);

                // Objects stat
                DrawStatBox("Objects", _totalDirtyObjects.ToString(), _gameObjectIcon);
                GUILayout.Space(20);

                // Components stat
                DrawStatBox("Components", _totalDirtyComponents.ToString(), _componentIcon);

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatBox(string label, string value, Texture2D icon)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(80));
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (icon != null)
                    {
                        GUILayout.Label(icon, GUILayout.Width(16), GUILayout.Height(16));
                    }
                    GUILayout.Label(value, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(label, EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label(EditorGUIUtility.IconContent("d_Search Icon"), GUILayout.Width(20), GUILayout.Height(16));
                _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField);

                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(45)))
                {
                    _searchFilter = "";
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDirtyObjectsList()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                if (_dirtyScenes.Count == 0)
                {
                    DrawEmptyState();
                }
                else
                {
                    foreach (var sceneInfo in _dirtyScenes)
                    {
                        // Apply scene filter
                        if (_selectedSceneIndex > 0 && _sceneFilterOptions[_selectedSceneIndex] != sceneInfo.SceneName)
                            continue;

                        DrawSceneSection(sceneInfo);
                    }
                }

                GUILayout.Space(50);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawEmptyState()
        {
            GUILayout.Space(30);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginVertical(GUILayout.Width(250));
                {
                    GUILayout.Label(EditorGUIUtility.IconContent("d_Valid@2x"), GUILayout.Width(48), GUILayout.Height(48));
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("No Dirty Objects Found", _headerStyle);
                    EditorGUILayout.LabelField("All objects in loaded scenes are clean.",
                        EditorStyles.wordWrappedMiniLabel);
                }
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSceneSection(DirtySceneInfo sceneInfo)
        {
            // Scene header
            Color originalBg = GUI.backgroundColor;
            GUI.backgroundColor = SceneHeaderColor;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUI.backgroundColor = originalBg;

                EditorGUILayout.BeginHorizontal();
                {
                    // Foldout
                    sceneInfo.IsExpanded = EditorGUILayout.Foldout(sceneInfo.IsExpanded,
                        GUIContent.none, true, _foldoutStyle);

                    // Scene icon and name
                    if (_sceneIcon != null)
                    {
                        GUILayout.Label(_sceneIcon, GUILayout.Width(16), GUILayout.Height(16));
                    }

                    EditorGUILayout.LabelField(sceneInfo.SceneName, _sceneLabelStyle, GUILayout.ExpandWidth(true));

                    // Count badge
                    GUILayout.Label($"({sceneInfo.TotalDirtyObjectCount} objects)", EditorStyles.miniLabel);

                    // Save scene button
                    if (GUILayout.Button(new GUIContent(_saveIcon, "Save this scene"),
                            EditorStyles.miniButton, GUILayout.Width(25), GUILayout.Height(18)))
                    {
                        EditorSceneManager.SaveScene(sceneInfo.Scene);
                        RefreshDirtyObjects();
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (sceneInfo.IsExpanded)
                {
                    EditorGUI.indentLevel++;

                    foreach (var goInfo in sceneInfo.DirtyGameObjects)
                    {
                        if (!MatchesFilter(goInfo.GameObject.name)) continue;
                        DrawGameObjectSection(goInfo);
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(2);
        }

        private void DrawGameObjectSection(DirtyGameObjectInfo goInfo)
        {
            if (goInfo.GameObject == null) return;

            Color originalBg = GUI.backgroundColor;
            GUI.backgroundColor = GameObjectHeaderColor;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUI.backgroundColor = originalBg;

                EditorGUILayout.BeginHorizontal();
                {
                    // Foldout (if has dirty components or dirty properties)
                    bool hasContent = goInfo.DirtyComponents.Count > 0 || goInfo.GameObjectDirtyProperties.Count > 0;
                    if (hasContent)
                    {
                        goInfo.IsExpanded = EditorGUILayout.Foldout(goInfo.IsExpanded,
                            GUIContent.none, true, EditorStyles.foldout);
                    }
                    else
                    {
                        GUILayout.Space(15);
                    }

                    // GameObject icon
                    if (_gameObjectIcon != null)
                    {
                        GUILayout.Label(_gameObjectIcon, GUILayout.Width(16), GUILayout.Height(16));
                    }

                    // GameObject name and path
                    string displayName = goInfo.GameObject.name;
                    if (goInfo.IsGameObjectDirty)
                    {
                        displayName += " *";
                    }

                    EditorGUILayout.LabelField(new GUIContent(displayName, GetGameObjectPath(goInfo.GameObject)),
                        EditorStyles.label, GUILayout.ExpandWidth(true));

                    // Component count badge
                    if (goInfo.DirtyComponents.Count > 0)
                    {
                        GUILayout.Label($"({goInfo.DirtyComponents.Count} comps)", EditorStyles.miniLabel);
                    }

                    // GameObject properties count badge
                    if (goInfo.GameObjectDirtyProperties.Count > 0)
                    {
                        GUILayout.Label($"({goInfo.GameObjectDirtyProperties.Count} props)", EditorStyles.miniLabel);
                    }

                    // Prefab badge
                    if (goInfo.IsPrefabInstance)
                    {
                        GUILayout.Label(new GUIContent(_prefabIcon, "Prefab Instance"), GUILayout.Width(16), GUILayout.Height(16));
                    }

                    // Ping button
                    if (GUILayout.Button(new GUIContent(_pingIcon, "Ping in Hierarchy"),
                            EditorStyles.miniButton, GUILayout.Width(25), GUILayout.Height(18)))
                    {
                        EditorGUIUtility.PingObject(goInfo.GameObject);
                        Selection.activeGameObject = goInfo.GameObject;
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Draw expanded content
                if (goInfo.IsExpanded)
                {
                    EditorGUI.indentLevel++;

                    // Draw GameObject's own dirty properties first
                    if (goInfo.GameObjectDirtyProperties.Count > 0)
                    {
                        EditorGUILayout.BeginVertical();
                        {
                            GUILayout.Label("GameObject Properties:", EditorStyles.miniBoldLabel);
                            DrawDirtyProperties(goInfo.GameObjectDirtyProperties);
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5);
                    }

                    // Draw dirty components
                    foreach (var compInfo in goInfo.DirtyComponents)
                    {
                        DrawComponentItem(compInfo, goInfo.GameObject);
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawComponentItem(DirtyComponentInfo compInfo, GameObject parentGo)
        {
            if (compInfo.Component == null) return;

            Color originalBg = GUI.backgroundColor;
            GUI.backgroundColor = ComponentItemColor;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUI.backgroundColor = originalBg;

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(10);

                    // Foldout for properties (if any)
                    if (compInfo.DirtyProperties.Count > 0)
                    {
                        compInfo.IsExpanded = EditorGUILayout.Foldout(compInfo.IsExpanded,
                            GUIContent.none, true, EditorStyles.foldout);
                    }
                    else
                    {
                        GUILayout.Space(15);
                    }

                    // Component icon
                    Texture2D compIcon = EditorGUIUtility.ObjectContent(compInfo.Component, compInfo.Component.GetType()).image as Texture2D;
                    if (compIcon != null)
                    {
                        GUILayout.Label(compIcon, GUILayout.Width(16), GUILayout.Height(16));
                    }
                    else if (_componentIcon != null)
                    {
                        GUILayout.Label(_componentIcon, GUILayout.Width(16), GUILayout.Height(16));
                    }

                    // Component name
                    EditorGUILayout.LabelField(compInfo.TypeName, EditorStyles.label, GUILayout.ExpandWidth(true));

                    // Property count badge
                    if (compInfo.DirtyProperties.Count > 0)
                    {
                        GUILayout.Label($"({compInfo.DirtyProperties.Count} props)", EditorStyles.miniLabel);
                    }

                    // Prefab badge
                    if (compInfo.IsPrefabInstance)
                    {
                        GUILayout.Label(new GUIContent(_prefabIcon, "Prefab Instance"), GUILayout.Width(16), GUILayout.Height(16));
                    }

                    // Select component button
                    if (GUILayout.Button(new GUIContent(_pingIcon, "Select Component"),
                            EditorStyles.miniButton, GUILayout.Width(25), GUILayout.Height(16)))
                    {
                        Selection.activeGameObject = parentGo;
                        EditorGUIUtility.PingObject(compInfo.Component);
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Draw dirty properties if expanded
                if (compInfo.IsExpanded && compInfo.DirtyProperties.Count > 0)
                {
                    DrawDirtyProperties(compInfo.DirtyProperties);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawDirtyProperties(List<DirtyPropertyInfo> properties)
        {
            EditorGUI.indentLevel++;

            foreach (var prop in properties)
            {
                Color originalBg = GUI.backgroundColor;
                GUI.backgroundColor = prop.IsPrefabOverride ? PrefabOverrideColor : PropertyItemColor;

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    GUI.backgroundColor = originalBg;

                    GUILayout.Space(30);

                    // Property icon
                    if (_propertyIcon != null)
                    {
                        GUILayout.Label(_propertyIcon, GUILayout.Width(14), GUILayout.Height(14));
                    }

                    // Property name
                    EditorGUILayout.LabelField(prop.DisplayName, _propertyNameStyle, GUILayout.Width(150));

                    // Current value
                    GUILayout.Label("=", GUILayout.Width(15));
                    EditorGUILayout.LabelField(prop.Value, _propertyValueStyle, GUILayout.MinWidth(80));

                    // Show original value for prefab overrides
                    if (prop.IsPrefabOverride && !string.IsNullOrEmpty(prop.OriginalValue))
                    {
                        GUILayout.Label(new GUIContent("(was: " + prop.OriginalValue + ")",
                            "Original prefab value"), EditorStyles.miniLabel, GUILayout.MaxWidth(120));
                    }

                    // Prefab override indicator
                    if (prop.IsPrefabOverride)
                    {
                        GUILayout.Label("[Override]", _prefabBadgeStyle, GUILayout.Width(55));
                    }

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.FlexibleSpace();

                // Save All Dirty Scenes button
                EditorGUI.BeginDisabledGroup(_totalDirtyScenes == 0);
                if (GUILayout.Button(new GUIContent($" Save All ({_totalDirtyScenes} scenes)", _saveIcon,
                        "Save all scenes with dirty objects"), GUILayout.Height(22), GUILayout.Width(150)))
                {
                    foreach (var sceneInfo in _dirtyScenes)
                    {
                        EditorSceneManager.SaveScene(sceneInfo.Scene);
                    }
                    RefreshDirtyObjects();
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion
    }
}
#endif