#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Utils.Editor
{
    /// <summary>
    /// Editor window that allows viewing and editing Unity's default script templates.
    /// Works on both Windows and macOS.
    /// </summary>
    public class ScriptTemplateEditorWindow : EditorWindow
    {
        #region Data Classes

        private class ScriptTemplateInfo
        {
            public string FileName;
            public string FilePath;
            public string DisplayName;
            public string Category;
            public string Content;
            public string OriginalContent;
            public bool HasUnsavedChanges;
            public bool IsReadOnly;
        }

        #endregion

        #region Private Fields

        private List<ScriptTemplateInfo> _templates = new List<ScriptTemplateInfo>();
        private ScriptTemplateInfo _selectedTemplate;
        private Vector2 _listScrollPosition;
        private Vector2 _editorScrollPosition;
        private string _searchFilter = "";
        private string _templatesPath = "";
        private bool _templatesPathValid;
        private string _selectedCategory = "All";
        private List<string> _categories = new List<string> { "All" };

        // Styles
        private GUIStyle _headerStyle;
        private GUIStyle _templateListItemStyle;
        private GUIStyle _templateListItemSelectedStyle;
        private GUIStyle _categoryLabelStyle;
        private GUIStyle _pathLabelStyle;
        private GUIStyle _editorTextAreaStyle;
        private GUIStyle _toolbarButtonStyle;
        private GUIStyle _unsavedBadgeStyle;
        private GUIStyle _readOnlyBadgeStyle;
        private GUIStyle _infoBoxStyle;
        private bool _stylesInitialized;

        // Colors
        private static readonly Color SelectedItemColor = new Color(0.24f, 0.49f, 0.91f, 0.5f);
        private static readonly Color HoverItemColor = new Color(0.4f, 0.4f, 0.4f, 0.2f);
        private static readonly Color UnsavedColor = new Color(1f, 0.8f, 0.2f, 1f);
        private static readonly Color ReadOnlyColor = new Color(1f, 0.4f, 0.4f, 1f);
        private static readonly Color CategoryHeaderColor = new Color(0.3f, 0.5f, 0.7f, 0.3f);
        private static readonly Color EditorBackgroundColor = new Color(0.16f, 0.16f, 0.16f, 1f);

        // Syntax highlighting colors (VS Code Dark+ theme inspired)
        private static readonly Color SyntaxKeywordColor = new Color(0.34f, 0.61f, 0.84f, 1f); // Blue - keywords
        private static readonly Color SyntaxTypeColor = new Color(0.31f, 0.78f, 0.78f, 1f); // Cyan - types
        private static readonly Color SyntaxStringColor = new Color(0.81f, 0.54f, 0.46f, 1f); // Orange - strings
        private static readonly Color SyntaxCommentColor = new Color(0.42f, 0.54f, 0.35f, 1f); // Green - comments
        private static readonly Color SyntaxNumberColor = new Color(0.71f, 0.80f, 0.55f, 1f); // Light green - numbers
        private static readonly Color SyntaxMethodColor = new Color(0.86f, 0.86f, 0.67f, 1f); // Yellow - methods
        private static readonly Color SyntaxVariableColor = new Color(0.61f, 0.75f, 0.86f, 1f); // Light blue - template vars
        private static readonly Color SyntaxDefaultColor = new Color(0.86f, 0.86f, 0.86f, 1f); // Light gray - default
        private static readonly Color LineNumberColor = new Color(0.45f, 0.45f, 0.45f, 1f); // Gray - line numbers
        private static readonly Color LineNumberBgColor = new Color(0.14f, 0.14f, 0.14f, 1f); // Dark - line number bg
        private static readonly Color CurrentLineColor = new Color(0.22f, 0.22f, 0.22f, 1f); // Slightly lighter - current line

        // Line number width
        private const float LINE_NUMBER_WIDTH = 50f;

        // Icons
        private Texture2D _scriptIcon;
        private Texture2D _folderIcon;
        private Texture2D _saveIcon;
        private Texture2D _refreshIcon;
        private Texture2D _revertIcon;
        private Texture2D _searchIcon;
        private Texture2D _warningIcon;

        // Layout
        private float _listPanelWidth = 280f;
        private bool _isResizingPanel;
        private const float MIN_LIST_PANEL_WIDTH = 200f;
        private const float MAX_LIST_PANEL_WIDTH = 400f;
        private const float RESIZER_WIDTH = 4f;

        // Code editor
        private GUIStyle _lineNumberStyle;
        private GUIStyle _codeLineStyle;
        private GUIStyle _codeEditorStyle;

        // C# Keywords for syntax highlighting
        private static readonly HashSet<string> CSharpKeywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
            "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
            "void", "volatile", "while", "var", "async", "await", "partial", "where", "get", "set",
            "add", "remove", "yield", "dynamic", "nameof", "when", "record"
        };

        private static readonly HashSet<string> CSharpTypes = new HashSet<string>
        {
            "MonoBehaviour", "ScriptableObject", "GameObject", "Transform", "Component", "Rigidbody",
            "Collider", "Vector2", "Vector3", "Vector4", "Quaternion", "Color", "Rect", "Bounds",
            "String", "Int32", "Int64", "Single", "Double", "Boolean", "List", "Dictionary",
            "IEnumerable", "IEnumerator", "Action", "Func", "Task", "Exception", "Debug"
        };

        #endregion

        #region Menu Item & Window Setup

        [MenuItem("Tools/Script Template Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<ScriptTemplateEditorWindow>();
            window.titleContent = new GUIContent("Script Template Editor", EditorGUIUtility.IconContent("cs Script Icon").image);
            window.minSize = new Vector2(800, 500);
            window.Show();
        }

        private void OnEnable()
        {
            LoadIcons();
            FindTemplatesPath();
            LoadTemplates();
        }

        private void OnDisable()
        {
            // Check for unsaved changes
            if (HasUnsavedChanges())
            {
                if (EditorUtility.DisplayDialog("Unsaved Changes",
                        "You have unsaved changes. Do you want to save them before closing?",
                        "Save", "Discard"))
                {
                    SaveAllChanges();
                }
            }
        }

        private void LoadIcons()
        {
            _scriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
            _folderIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
            _saveIcon = EditorGUIUtility.IconContent("SaveAs").image as Texture2D;
            _refreshIcon = EditorGUIUtility.IconContent("d_Refresh").image as Texture2D;
            _revertIcon = EditorGUIUtility.IconContent("d_preAudioLoopOff").image as Texture2D;
            _searchIcon = EditorGUIUtility.IconContent("d_Search Icon").image as Texture2D;
            _warningIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image as Texture2D;
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

            _templateListItemStyle = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(8, 8, 4, 4),
                margin = new RectOffset(2, 2, 1, 1),
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 24
            };

            _templateListItemSelectedStyle = new GUIStyle(_templateListItemStyle)
            {
                fontStyle = FontStyle.Bold
            };

            _categoryLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                padding = new RectOffset(4, 4, 6, 4)
            };

            _pathLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true,
                richText = true
            };

            _editorTextAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                font = GetMonospaceFont(),
                fontSize = 12,
                padding = new RectOffset(8, 8, 8, 8),
                wordWrap = false
            };

            _toolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                padding = new RectOffset(8, 8, 2, 2)
            };

            _unsavedBadgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = UnsavedColor },
                fontStyle = FontStyle.Bold,
                fontSize = 10
            };

            _readOnlyBadgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = ReadOnlyColor },
                fontStyle = FontStyle.Bold,
                fontSize = 10
            };

            _infoBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 8, 8),
                richText = true
            };

            // Code editor styles
            Font monoFont = GetMonospaceFont();

            _lineNumberStyle = new GUIStyle(EditorStyles.label)
            {
                font = monoFont,
                fontSize = 12,
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(4, 8, 0, 0),
                normal = { textColor = LineNumberColor },
                richText = false
            };

            _codeLineStyle = new GUIStyle(EditorStyles.label)
            {
                font = monoFont,
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 8, 0, 0),
                richText = true,
                wordWrap = false
            };

            _codeEditorStyle = new GUIStyle(EditorStyles.textArea)
            {
                font = monoFont,
                fontSize = 12,
                padding = new RectOffset(8, 8, 4, 4),
                wordWrap = false,
                richText = false
            };

            _stylesInitialized = true;
        }

        private Font GetMonospaceFont()
        {
            // Try to use a monospace font for better code editing
            Font font = EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf") as Font;
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("Consolas", 12);
            }
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("Menlo", 12);
            }
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("Monaco", 12);
            }
            return font;
        }

        #endregion

        #region Path Detection

        private void FindTemplatesPath()
        {
            // EditorApplication.applicationPath returns the path to the CURRENTLY RUNNING Unity Editor
            // This works correctly regardless of which Unity version is installed or where
            string unityEditorPath = EditorApplication.applicationPath;

            Debug.Log($"[ScriptTemplateEditor] Unity Editor Path: {unityEditorPath}");

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Windows: {Unity Editor Folder}/Data/Resources/ScriptTemplates/
                // applicationPath returns something like: C:/Program Files/Unity/Hub/Editor/2022.3.10f1/Editor/Unity.exe
                string editorFolder = Path.GetDirectoryName(unityEditorPath);
                _templatesPath = Path.Combine(editorFolder, "Data", "Resources", "ScriptTemplates");
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                // macOS: {Unity.app}/Contents/Resources/ScriptTemplates/
                // applicationPath returns something like: /Applications/Unity/Hub/Editor/2022.3.10f1/Unity.app
                // We need to go inside the .app bundle to find Resources
                _templatesPath = Path.Combine(unityEditorPath, "Contents", "Resources", "ScriptTemplates");
            }
            else
            {
                // Linux: Similar to Windows structure
                string editorFolder = Path.GetDirectoryName(unityEditorPath);
                _templatesPath = Path.Combine(editorFolder, "Data", "Resources", "ScriptTemplates");
            }

            // Normalize path separators
            _templatesPath = _templatesPath.Replace("\\", "/");
            _templatesPathValid = Directory.Exists(_templatesPath);

            Debug.Log($"[ScriptTemplateEditor] Templates Path: {_templatesPath} (Valid: {_templatesPathValid})");
        }

        #endregion

        #region Template Loading

        private void LoadTemplates()
        {
            _templates.Clear();
            _categories.Clear();
            _categories.Add("All");

            if (!_templatesPathValid)
            {
                return;
            }

            try
            {
                string[] templateFiles = Directory.GetFiles(_templatesPath, "*.txt");

                foreach (string filePath in templateFiles)
                {
                    string fileName = Path.GetFileName(filePath);
                    string content = File.ReadAllText(filePath);

                    // Parse template info from filename
                    // Unity template naming: XX-MenuName__TemplateName-FileName.txt
                    var templateInfo = ParseTemplateFileName(fileName, filePath, content);

                    // Check if file is read-only
                    FileInfo fileInfo = new FileInfo(filePath);
                    templateInfo.IsReadOnly = fileInfo.IsReadOnly;

                    _templates.Add(templateInfo);

                    // Add category if not exists
                    if (!_categories.Contains(templateInfo.Category))
                    {
                        _categories.Add(templateInfo.Category);
                    }
                }

                // Sort templates by category then by display name
                _templates = _templates
                    .OrderBy(t => t.Category)
                    .ThenBy(t => t.DisplayName)
                    .ToList();

                // Select first template if available
                if (_templates.Count > 0 && _selectedTemplate == null)
                {
                    _selectedTemplate = _templates[0];
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScriptTemplateEditor] Error loading templates: {ex.Message}");
            }
        }

        private ScriptTemplateInfo ParseTemplateFileName(string fileName, string filePath, string content)
        {
            // Unity template naming convention: XX-Category__DisplayName-FileName.cs.txt
            // Examples:
            // 81-C# Script-NewBehaviourScript.cs.txt
            // 82-Javascript-NewBehaviourScript.js.txt
            // 83-Shader__Standard Surface Shader-NewSurfaceShader.shader.txt

            string displayName = fileName;
            string category = "Other";

            try
            {
                // Remove .txt extension
                string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

                // Split by first dash to get priority number
                int firstDash = nameWithoutExt.IndexOf('-');
                if (firstDash > 0)
                {
                    string afterPriority = nameWithoutExt.Substring(firstDash + 1);

                    // Check for double underscore (category separator)
                    int categoryEnd = afterPriority.IndexOf("__", StringComparison.Ordinal);
                    if (categoryEnd > 0)
                    {
                        category = afterPriority.Substring(0, categoryEnd);
                        afterPriority = afterPriority.Substring(categoryEnd + 2);
                    }
                    else
                    {
                        // Single category/name format
                        int lastDash = afterPriority.LastIndexOf('-');
                        if (lastDash > 0)
                        {
                            category = afterPriority.Substring(0, lastDash);
                        }
                    }

                    // Extract display name (remove the template file name at the end)
                    int nameDash = afterPriority.LastIndexOf('-');
                    if (nameDash > 0)
                    {
                        displayName = afterPriority.Substring(0, nameDash);
                    }
                    else
                    {
                        displayName = afterPriority;
                    }
                }
            }
            catch
            {
                // If parsing fails, use filename as display name
                displayName = fileName;
            }

            return new ScriptTemplateInfo
            {
                FileName = fileName,
                FilePath = filePath,
                DisplayName = displayName.Replace("__", " > "),
                Category = category,
                Content = content,
                OriginalContent = content,
                HasUnsavedChanges = false
            };
        }

        #endregion

        #region GUI Drawing

        private void OnGUI()
        {
            InitializeStyles();

            if (!_templatesPathValid)
            {
                DrawPathNotFoundError();
                return;
            }

            DrawToolbar();

            EditorGUILayout.BeginHorizontal();
            {
                // Left panel - Template list
                DrawTemplateListPanel();

                // Resizer
                DrawResizer();

                // Right panel - Editor
                DrawEditorPanel();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPathNotFoundError()
        {
            GUILayout.Space(50);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(600));
                {
                    // Error icon
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(EditorGUIUtility.IconContent("console.erroricon@2x"), GUILayout.Width(48), GUILayout.Height(48));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(10);

                    EditorGUILayout.LabelField("Script Templates Not Found", _headerStyle);

                    GUILayout.Space(10);

                    string unityPath = EditorApplication.applicationPath;
                    EditorGUILayout.HelpBox(
                        $"Could not find Unity's script templates folder.\n\n" +
                        $"Unity Editor Path:\n{unityPath}\n\n" +
                        $"Expected Templates Path:\n{_templatesPath}\n\n" +
                        $"Platform: {Application.platform}\n\n" +
                        $"This might happen if:\n" +
                        $"• Unity is installed in a non-standard location\n" +
                        $"• The ScriptTemplates folder was moved or deleted\n" +
                        $"• Insufficient permissions to access the folder",
                        MessageType.Error);

                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Retry", GUILayout.Height(30)))
                        {
                            FindTemplatesPath();
                            LoadTemplates();
                        }

                        GUILayout.Space(10);

                        if (GUILayout.Button("Copy Paths to Clipboard", GUILayout.Height(30)))
                        {
                            string info = $"Unity Editor: {unityPath}\nTemplates Path: {_templatesPath}\nPlatform: {Application.platform}";
                            EditorGUIUtility.systemCopyBuffer = info;
                            Debug.Log("[ScriptTemplateEditor] Paths copied to clipboard");
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                // Templates path info
                GUILayout.Label(new GUIContent(_folderIcon), GUILayout.Width(18), GUILayout.Height(16));
                GUILayout.Label($"Templates: {_templatesPath}", EditorStyles.miniLabel, GUILayout.MaxWidth(position.width - 350));

                GUILayout.FlexibleSpace();

                // Show in Finder/Explorer button
                if (GUILayout.Button(new GUIContent(" Show in " + GetFileExplorerName(), _folderIcon, "Open templates folder in file explorer"),
                        _toolbarButtonStyle, GUILayout.Width(130)))
                {
                    RevealInFinder(_templatesPath);
                }

                GUILayout.Space(5);

                // Refresh button
                if (GUILayout.Button(new GUIContent(" Refresh", _refreshIcon, "Reload all templates"),
                        _toolbarButtonStyle, GUILayout.Width(80)))
                {
                    LoadTemplates();
                }

                GUILayout.Space(5);

                // Save All button
                bool hasChanges = HasUnsavedChanges();
                EditorGUI.BeginDisabledGroup(!hasChanges);
                if (GUILayout.Button(new GUIContent(" Save All", _saveIcon, "Save all modified templates"),
                        _toolbarButtonStyle, GUILayout.Width(80)))
                {
                    SaveAllChanges();
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTemplateListPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(_listPanelWidth));
            {
                // Search bar
                DrawSearchBar();

                // Category filter
                DrawCategoryFilter();

                // Template list
                _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition, GUILayout.ExpandHeight(true));
                {
                    DrawTemplateList();
                }
                EditorGUILayout.EndScrollView();

                // Stats footer
                DrawListFooter();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label(_searchIcon, GUILayout.Width(18), GUILayout.Height(16));

                string newFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField);
                if (newFilter != _searchFilter)
                {
                    _searchFilter = newFilter;
                }

                if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    _searchFilter = "";
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCategoryFilter()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("Category:", EditorStyles.miniLabel, GUILayout.Width(55));

                int selectedIndex = _categories.IndexOf(_selectedCategory);
                if (selectedIndex < 0) selectedIndex = 0;

                int newIndex = EditorGUILayout.Popup(selectedIndex, _categories.ToArray(), EditorStyles.toolbarPopup);
                if (newIndex != selectedIndex)
                {
                    _selectedCategory = _categories[newIndex];
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTemplateList()
        {
            var filteredTemplates = _templates.Where(t =>
                (_selectedCategory == "All" || t.Category == _selectedCategory) &&
                (string.IsNullOrEmpty(_searchFilter) ||
                 t.DisplayName.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 t.FileName.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();

            if (filteredTemplates.Count == 0)
            {
                GUILayout.Space(20);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("No templates match the filter", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                return;
            }

            // Group by category
            var groupedTemplates = filteredTemplates.GroupBy(t => t.Category);

            foreach (var group in groupedTemplates)
            {
                // Draw category header
                Color originalBg = GUI.backgroundColor;
                GUI.backgroundColor = CategoryHeaderColor;

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    GUI.backgroundColor = originalBg;
                    GUILayout.Label(group.Key, _categoryLabelStyle);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"({group.Count()})", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndHorizontal();

                // Draw templates in this category
                foreach (var template in group)
                {
                    DrawTemplateListItem(template);
                }

                GUILayout.Space(5);
            }
        }

        private void DrawTemplateListItem(ScriptTemplateInfo template)
        {
            bool isSelected = _selectedTemplate == template;

            // Create rect for the item
            Rect itemRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(24));
            {
                // Draw background
                if (Event.current.type == EventType.Repaint)
                {
                    Color bgColor = isSelected ? SelectedItemColor : Color.clear;
                    if (!isSelected && itemRect.Contains(Event.current.mousePosition))
                    {
                        bgColor = HoverItemColor;
                    }

                    if (bgColor != Color.clear)
                    {
                        EditorGUI.DrawRect(itemRect, bgColor);
                    }
                }

                // Handle click
                if (Event.current.type == EventType.MouseDown && itemRect.Contains(Event.current.mousePosition))
                {
                    _selectedTemplate = template;
                    Event.current.Use();
                    Repaint();
                }

                GUILayout.Space(8);

                // Script icon
                GUILayout.Label(_scriptIcon, GUILayout.Width(16), GUILayout.Height(16));
                GUILayout.Space(4);

                // Template name
                var style = isSelected ? _templateListItemSelectedStyle : _templateListItemStyle;
                GUILayout.Label(template.DisplayName, style, GUILayout.ExpandWidth(true));

                // Badges
                if (template.HasUnsavedChanges)
                {
                    GUILayout.Label("*", _unsavedBadgeStyle, GUILayout.Width(10));
                }

                if (template.IsReadOnly)
                {
                    GUILayout.Label("RO", _readOnlyBadgeStyle, GUILayout.Width(20));
                }

                GUILayout.Space(4);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawListFooter()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                int unsavedCount = _templates.Count(t => t.HasUnsavedChanges);
                string statusText = $"{_templates.Count} templates";
                if (unsavedCount > 0)
                {
                    statusText += $" | {unsavedCount} unsaved";
                }
                GUILayout.Label(statusText, EditorStyles.centeredGreyMiniLabel);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawResizer()
        {
            Rect resizerRect = new Rect(_listPanelWidth, 0, RESIZER_WIDTH, position.height);
            EditorGUIUtility.AddCursorRect(resizerRect, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.MouseDown && resizerRect.Contains(Event.current.mousePosition))
            {
                _isResizingPanel = true;
                Event.current.Use();
            }

            if (_isResizingPanel)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    _listPanelWidth = Mathf.Clamp(Event.current.mousePosition.x, MIN_LIST_PANEL_WIDTH, MAX_LIST_PANEL_WIDTH);
                    Repaint();
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    _isResizingPanel = false;
                }
            }

            // Draw resizer line
            EditorGUI.DrawRect(new Rect(_listPanelWidth, 0, 1, position.height), new Color(0.1f, 0.1f, 0.1f, 1f));
        }

        private void DrawEditorPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                if (_selectedTemplate == null)
                {
                    DrawNoTemplateSelected();
                }
                else
                {
                    DrawEditorHeader();
                    DrawEditorContent();
                    DrawEditorFooter();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawNoTemplateSelected()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Select a template from the list to edit", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        private void DrawEditorHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                // Template name
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(_scriptIcon, GUILayout.Width(20), GUILayout.Height(20));
                    GUILayout.Label(_selectedTemplate.DisplayName, _headerStyle);

                    GUILayout.FlexibleSpace();

                    if (_selectedTemplate.HasUnsavedChanges)
                    {
                        GUILayout.Label(_warningIcon, GUILayout.Width(16), GUILayout.Height(16));
                        GUILayout.Label("Unsaved changes", _unsavedBadgeStyle);
                    }

                    if (_selectedTemplate.IsReadOnly)
                    {
                        GUILayout.Label("Read-Only", _readOnlyBadgeStyle);
                    }
                }
                EditorGUILayout.EndHorizontal();

                // File path
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(24);
                    GUILayout.Label($"<color=#888888>{_selectedTemplate.FilePath}</color>", _pathLabelStyle);

                    GUILayout.FlexibleSpace();

                    // Show in Finder button for specific file
                    if (GUILayout.Button(new GUIContent("Show", _folderIcon, "Reveal file in " + GetFileExplorerName()),
                            EditorStyles.miniButton, GUILayout.Width(60)))
                    {
                        RevealInFinder(_selectedTemplate.FilePath);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(2);
        }

        private void DrawEditorContent()
        {
            // Info box about template variables
            EditorGUILayout.BeginHorizontal(_infoBoxStyle);
            {
                GUILayout.Label(EditorGUIUtility.IconContent("d_console.infoicon.sml"), GUILayout.Width(16), GUILayout.Height(16));
                GUILayout.Label("<b>Variables:</b> #SCRIPTNAME#, #ROOTNAMESPACEBEGIN#, #ROOTNAMESPACEEND#, #NOTRIM#",
                    new GUIStyle(EditorStyles.miniLabel) { richText = true });
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);

            // Main editor area with line numbers and syntax highlighting
            DrawCodeEditorWithLineNumbers();
        }

        private void DrawCodeEditorWithLineNumbers()
        {
            string[] lines = _selectedTemplate.Content.Split('\n');
            int lineCount = lines.Length;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float contentHeight = lineCount * lineHeight + 40f;

            // Calculate required width for line numbers
            float lineNumWidth = Mathf.Max(LINE_NUMBER_WIDTH, (lineCount.ToString().Length * 10f) + 20f);

            // Main container with dark background
            Rect containerRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(containerRect, EditorBackgroundColor);

            // Begin scroll view
            _editorScrollPosition = EditorGUILayout.BeginScrollView(_editorScrollPosition,
                false, false,
                GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            {
                EditorGUILayout.BeginHorizontal();
                {
                    // === LEFT PANEL: Line Numbers ===
                    EditorGUILayout.BeginVertical(GUILayout.Width(lineNumWidth));
                    {
                        // Draw each line number
                        for (int i = 0; i < lineCount; i++)
                        {
                            Rect lineRect = GUILayoutUtility.GetRect(lineNumWidth, lineHeight);

                            // Draw line number background
                            EditorGUI.DrawRect(lineRect, LineNumberBgColor);

                            // Draw line number
                            GUI.Label(lineRect, (i + 1).ToString(), _lineNumberStyle);
                        }
                    }
                    EditorGUILayout.EndVertical();

                    // Separator line
                    Rect separatorRect = GUILayoutUtility.GetRect(1f, contentHeight);
                    EditorGUI.DrawRect(separatorRect, new Color(0.3f, 0.3f, 0.3f, 1f));

                    // === RIGHT PANEL: Syntax Highlighted Code ===
                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    {
                        // Draw syntax highlighted code (read-only preview)
                        DrawSyntaxHighlightedLines(lines, lineHeight);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            GUILayout.Space(4);

            // === EDITABLE TEXT AREA BELOW ===
            DrawEditableSection();
        }

        private void DrawSyntaxHighlightedLines(string[] lines, float lineHeight)
        {
            bool inMultiLineComment = false;

            for (int i = 0; i < lines.Length; i++)
            {
                Rect lineRect = GUILayoutUtility.GetRect(10f, lineHeight, GUILayout.ExpandWidth(true));

                // Draw alternating line background
                if (i % 2 == 0)
                {
                    EditorGUI.DrawRect(lineRect, new Color(0.18f, 0.18f, 0.18f, 1f));
                }
                else
                {
                    EditorGUI.DrawRect(lineRect, new Color(0.16f, 0.16f, 0.16f, 1f));
                }

                string highlightedLine = ApplySyntaxHighlighting(lines[i], ref inMultiLineComment);
                GUI.Label(lineRect, highlightedLine, _codeLineStyle);
            }
        }

        private void DrawEditableSection()
        {
            // Header for edit section
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label(EditorGUIUtility.IconContent("d_editicon.sml"), GUILayout.Width(16), GUILayout.Height(16));
                GUILayout.Label("Edit Template:", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();

                if (_selectedTemplate.IsReadOnly)
                {
                    GUILayout.Label("(Read-Only)", _readOnlyBadgeStyle);
                }
            }
            EditorGUILayout.EndHorizontal();

            // Editable text area with monospace font
            Color originalBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);

            EditorGUI.BeginDisabledGroup(_selectedTemplate.IsReadOnly);
            {
                EditorGUI.BeginChangeCheck();

                string newContent = EditorGUILayout.TextArea(
                    _selectedTemplate.Content,
                    _codeEditorStyle,
                    GUILayout.MinHeight(150f),
                    GUILayout.ExpandWidth(true));

                if (EditorGUI.EndChangeCheck())
                {
                    _selectedTemplate.Content = newContent;
                    _selectedTemplate.HasUnsavedChanges = _selectedTemplate.Content != _selectedTemplate.OriginalContent;
                }
            }
            EditorGUI.EndDisabledGroup();

            GUI.backgroundColor = originalBg;
        }

        private string ApplySyntaxHighlighting(string line, ref bool inMultiLineComment)
        {
            if (string.IsNullOrEmpty(line))
                return "";

            StringBuilder result = new StringBuilder();
            int i = 0;

            while (i < line.Length)
            {
                // Check for multi-line comment end
                if (inMultiLineComment)
                {
                    int endIndex = line.IndexOf("*/", i, StringComparison.Ordinal);
                    if (endIndex >= 0)
                    {
                        result.Append(ColorizeText(line.Substring(i, endIndex - i + 2), SyntaxCommentColor));
                        i = endIndex + 2;
                        inMultiLineComment = false;
                    }
                    else
                    {
                        result.Append(ColorizeText(line.Substring(i), SyntaxCommentColor));
                        return result.ToString();
                    }
                    continue;
                }

                // Check for multi-line comment start
                if (i < line.Length - 1 && line[i] == '/' && line[i + 1] == '*')
                {
                    int endIndex = line.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    if (endIndex >= 0)
                    {
                        result.Append(ColorizeText(line.Substring(i, endIndex - i + 2), SyntaxCommentColor));
                        i = endIndex + 2;
                    }
                    else
                    {
                        result.Append(ColorizeText(line.Substring(i), SyntaxCommentColor));
                        inMultiLineComment = true;
                        return result.ToString();
                    }
                    continue;
                }

                // Check for single-line comment
                if (i < line.Length - 1 && line[i] == '/' && line[i + 1] == '/')
                {
                    result.Append(ColorizeText(line.Substring(i), SyntaxCommentColor));
                    return result.ToString();
                }

                // Check for string literals
                if (line[i] == '"')
                {
                    int endIndex = FindStringEnd(line, i + 1);
                    result.Append(ColorizeText(line.Substring(i, endIndex - i + 1), SyntaxStringColor));
                    i = endIndex + 1;
                    continue;
                }

                // Check for template variables like #SCRIPTNAME#
                if (line[i] == '#')
                {
                    int endIndex = line.IndexOf('#', i + 1);
                    if (endIndex > i)
                    {
                        result.Append(ColorizeText(line.Substring(i, endIndex - i + 1), SyntaxVariableColor));
                        i = endIndex + 1;
                        continue;
                    }
                }

                // Check for words (keywords, types, identifiers)
                if (char.IsLetter(line[i]) || line[i] == '_')
                {
                    int wordEnd = i;
                    while (wordEnd < line.Length && (char.IsLetterOrDigit(line[wordEnd]) || line[wordEnd] == '_'))
                    {
                        wordEnd++;
                    }

                    string word = line.Substring(i, wordEnd - i);

                    if (CSharpKeywords.Contains(word))
                    {
                        result.Append(ColorizeText(word, SyntaxKeywordColor));
                    }
                    else if (CSharpTypes.Contains(word))
                    {
                        result.Append(ColorizeText(word, SyntaxTypeColor));
                    }
                    else if (wordEnd < line.Length && line[wordEnd] == '(')
                    {
                        result.Append(ColorizeText(word, SyntaxMethodColor));
                    }
                    else
                    {
                        result.Append(ColorizeText(word, SyntaxDefaultColor));
                    }

                    i = wordEnd;
                    continue;
                }

                // Check for numbers
                if (char.IsDigit(line[i]))
                {
                    int numEnd = i;
                    while (numEnd < line.Length && (char.IsDigit(line[numEnd]) || line[numEnd] == '.' || line[numEnd] == 'f'))
                    {
                        numEnd++;
                    }

                    result.Append(ColorizeText(line.Substring(i, numEnd - i), SyntaxNumberColor));
                    i = numEnd;
                    continue;
                }

                // Default: just add the character
                result.Append(ColorizeText(line[i].ToString(), SyntaxDefaultColor));
                i++;
            }

            return result.ToString();
        }

        private int FindStringEnd(string line, int startIndex)
        {
            for (int i = startIndex; i < line.Length; i++)
            {
                if (line[i] == '"' && (i == startIndex || line[i - 1] != '\\'))
                {
                    return i;
                }
            }
            return line.Length - 1;
        }

        private string ColorizeText(string text, Color color)
        {
            // Escape special characters for rich text
            text = text.Replace("<", "&lt;").Replace(">", "&gt;");
            string hexColor = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hexColor}>{text}</color>";
        }

        private void DrawEditorFooter()
        {
            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            {
                // Line count info
                int lineCount = _selectedTemplate.Content.Split('\n').Length;
                int charCount = _selectedTemplate.Content.Length;
                GUILayout.Label($"Lines: {lineCount} | Characters: {charCount}", EditorStyles.miniLabel);

                GUILayout.FlexibleSpace();

                // Revert button
                EditorGUI.BeginDisabledGroup(!_selectedTemplate.HasUnsavedChanges);
                if (GUILayout.Button(new GUIContent(" Revert", _revertIcon, "Revert to original content"),
                        GUILayout.Width(80), GUILayout.Height(24)))
                {
                    if (EditorUtility.DisplayDialog("Revert Changes",
                            $"Revert '{_selectedTemplate.DisplayName}' to its original content?",
                            "Revert", "Cancel"))
                    {
                        _selectedTemplate.Content = _selectedTemplate.OriginalContent;
                        _selectedTemplate.HasUnsavedChanges = false;
                    }
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(5);

                // Save button
                EditorGUI.BeginDisabledGroup(!_selectedTemplate.HasUnsavedChanges || _selectedTemplate.IsReadOnly);
                if (GUILayout.Button(new GUIContent(" Save", _saveIcon, "Save this template"),
                        GUILayout.Width(80), GUILayout.Height(24)))
                {
                    SaveTemplate(_selectedTemplate);
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
        }

        #endregion

        #region Actions

        private bool HasUnsavedChanges()
        {
            return _templates.Any(t => t.HasUnsavedChanges);
        }

        private void SaveTemplate(ScriptTemplateInfo template)
        {
            try
            {
                // Check if file is read-only
                FileInfo fileInfo = new FileInfo(template.FilePath);
                if (fileInfo.IsReadOnly)
                {
                    // Try to remove read-only attribute
                    if (EditorUtility.DisplayDialog("File is Read-Only",
                            "This file is marked as read-only. Do you want to try removing the read-only flag?",
                            "Yes", "No"))
                    {
                        try
                        {
                            fileInfo.IsReadOnly = false;
                            template.IsReadOnly = false;
                        }
                        catch (Exception ex)
                        {
                            EditorUtility.DisplayDialog("Error",
                                $"Could not remove read-only flag: {ex.Message}\n\n" +
                                "You may need to run Unity as Administrator (Windows) or change file permissions manually.",
                                "OK");
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                File.WriteAllText(template.FilePath, template.Content);
                template.OriginalContent = template.Content;
                template.HasUnsavedChanges = false;

                Debug.Log($"[ScriptTemplateEditor] Saved template: {template.FileName}");
            }
            catch (UnauthorizedAccessException)
            {
                EditorUtility.DisplayDialog("Permission Denied",
                    "Could not save the template due to insufficient permissions.\n\n" +
                    GetPermissionHelpText(),
                    "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error",
                    $"Failed to save template: {ex.Message}",
                    "OK");
            }
        }

        private void SaveAllChanges()
        {
            var unsavedTemplates = _templates.Where(t => t.HasUnsavedChanges && !t.IsReadOnly).ToList();
            int savedCount = 0;
            int failedCount = 0;

            foreach (var template in unsavedTemplates)
            {
                try
                {
                    File.WriteAllText(template.FilePath, template.Content);
                    template.OriginalContent = template.Content;
                    template.HasUnsavedChanges = false;
                    savedCount++;
                }
                catch
                {
                    failedCount++;
                }
            }

            if (failedCount > 0)
            {
                EditorUtility.DisplayDialog("Save Results",
                    $"Saved {savedCount} template(s).\n{failedCount} template(s) failed to save.\n\n" +
                    GetPermissionHelpText(),
                    "OK");
            }
            else if (savedCount > 0)
            {
                Debug.Log($"[ScriptTemplateEditor] Saved {savedCount} template(s)");
            }
        }

        private void RevealInFinder(string path)
        {
            // Normalize path for the current platform
            string normalizedPath = path.Replace("/", Path.DirectorySeparatorChar.ToString())
                .Replace("\\", Path.DirectorySeparatorChar.ToString());

            Debug.Log($"[ScriptTemplateEditor] Revealing path: {normalizedPath}");

            try
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    // Windows: Use explorer.exe with /select to highlight the file
                    // Path must use backslashes for Windows
                    string windowsPath = normalizedPath.Replace("/", "\\");

                    if (File.Exists(windowsPath))
                    {
                        // /select requires the path without quotes but with backslashes
                        System.Diagnostics.Process.Start("explorer.exe", "/select," + windowsPath);
                    }
                    else if (Directory.Exists(windowsPath))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", windowsPath);
                    }
                    else
                    {
                        Debug.LogWarning($"[ScriptTemplateEditor] Path does not exist: {windowsPath}");
                    }
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    // macOS: Use open -R to reveal in Finder
                    if (File.Exists(normalizedPath))
                    {
                        System.Diagnostics.Process.Start("open", $"-R \"{normalizedPath}\"");
                    }
                    else if (Directory.Exists(normalizedPath))
                    {
                        System.Diagnostics.Process.Start("open", $"\"{normalizedPath}\"");
                    }
                    else
                    {
                        Debug.LogWarning($"[ScriptTemplateEditor] Path does not exist: {normalizedPath}");
                    }
                }
                else
                {
                    // Linux: Try xdg-open
                    if (Directory.Exists(normalizedPath))
                    {
                        System.Diagnostics.Process.Start("xdg-open", $"\"{normalizedPath}\"");
                    }
                    else if (File.Exists(normalizedPath))
                    {
                        string directory = Path.GetDirectoryName(normalizedPath);
                        System.Diagnostics.Process.Start("xdg-open", $"\"{directory}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScriptTemplateEditor] Failed to reveal in finder: {ex.Message}");
            }
        }

        private string GetFileExplorerName()
        {
            if (Application.platform == RuntimePlatform.OSXEditor)
                return "Finder";
            else if (Application.platform == RuntimePlatform.WindowsEditor)
                return "Explorer";
            else
                return "File Manager";
        }

        private string GetPermissionHelpText()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                return "Try running Unity as Administrator to save templates.";
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                return "You may need to change the file permissions:\n" +
                       $"sudo chmod 666 \"{_templatesPath}\"/*";
            }
            else
            {
                return "You may need to change the file permissions.";
            }
        }

        #endregion
    }
}
#endif