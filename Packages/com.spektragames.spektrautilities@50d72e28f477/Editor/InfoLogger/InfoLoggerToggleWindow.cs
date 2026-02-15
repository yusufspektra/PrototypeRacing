using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEditor;
using UnityEngine;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Sirenix.Utilities.Editor;

namespace SpektraGames.SpektraUtilities.Editor
{
    public class InfoLoggerToggleWindow : EditorWindow
    {
        private bool _refreshing = false;
        private Vector2 _scrollPos = Vector2.zero;

        private GUIStyle _refreshingTextGuiStyle;

        private GUIStyle RefreshingTextGuiStyle
        {
            get
            {
                if (_refreshingTextGuiStyle == null)
                {
                    _refreshingTextGuiStyle = new GUIStyle();
                    _refreshingTextGuiStyle.fontStyle = FontStyle.Bold;
                    _refreshingTextGuiStyle.alignment = TextAnchor.MiddleCenter;
                    _refreshingTextGuiStyle.normal.textColor = Color.white;
                    _refreshingTextGuiStyle.fontSize = 50;
                }

                return _refreshingTextGuiStyle;
            }
        }

        private GUIStyle _currentLoggersTextGuiStyle;

        private GUIStyle CurrentLoggersTextGuiStyle
        {
            get
            {
                if (_currentLoggersTextGuiStyle == null)
                {
                    _currentLoggersTextGuiStyle = new GUIStyle();
                    _currentLoggersTextGuiStyle.fontStyle = FontStyle.Bold;
                    _currentLoggersTextGuiStyle.alignment = TextAnchor.MiddleLeft;
                    _currentLoggersTextGuiStyle.normal.textColor = Color.white;
                    _currentLoggersTextGuiStyle.fontSize = 15;
                }

                return _currentLoggersTextGuiStyle;
            }
        }

        private GUIStyle _loggerNameTextGuiStyle;

        private GUIStyle LoggerNameTextGuiStyle
        {
            get
            {
                if (_loggerNameTextGuiStyle == null)
                {
                    _loggerNameTextGuiStyle = new GUIStyle();
                    _loggerNameTextGuiStyle.fontStyle = FontStyle.Bold;
                    _loggerNameTextGuiStyle.alignment = TextAnchor.MiddleLeft;
                    _loggerNameTextGuiStyle.normal.textColor = Color.white;
                    _loggerNameTextGuiStyle.fontSize = 12;
                }

                return _loggerNameTextGuiStyle;
            }
        }

        private GUIStyle _loggerPathTextGuiStyle;

        private GUIStyle LoggerPathTextGuiStyle
        {
            get
            {
                if (_loggerPathTextGuiStyle == null)
                {
                    _loggerPathTextGuiStyle = new GUIStyle();
                    _loggerPathTextGuiStyle.fontStyle = FontStyle.Bold;
                    _loggerPathTextGuiStyle.alignment = TextAnchor.MiddleLeft;
                    _loggerPathTextGuiStyle.normal.textColor = Color.white;
                    _loggerPathTextGuiStyle.fontSize = 9;
                }

                return _loggerPathTextGuiStyle;
            }
        }

        [MenuItem("Tools/Info Logger/Toggle Window")]
        internal static void InitWindow()
        {
            EditorWindow.GetWindow<InfoLoggerToggleWindow>("Info Logger Toggle", true);
        }

        private void OnEnable()
        {
            _refreshingTextGuiStyle = null;
            _currentLoggersTextGuiStyle = null;
            _loggerNameTextGuiStyle = null;
            _loggerPathTextGuiStyle = null;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Only works in editor mode", MessageType.Error);
            }

            using (new EditorGUI.DisabledScope(_refreshing || Application.isPlaying))
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("CURRENT LOGGERS", CurrentLoggersTextGuiStyle);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Force Reset", GUILayout.Width(80f)) &&
                        EditorUtility.DisplayDialog("Important", "Are you sure to lost all your settings?", "Yes", "Cancel"))
                    {
                        Refresh(true);
                    }

                    if (GUILayout.Button("Update", GUILayout.Width(80f)))
                    {
                        Refresh(false);
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Draw definitions
                EditorGUILayout.Space();

                // --- ScrollView Start ---
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

                var definitions = InfoLoggerSettings.Instance.definitions;

                for (var i = 0; i < definitions.Count; i++)
                {
                    using (new EditorGUI.DisabledScope(!definitions[i].isEnabled))
                    {
                        Rect rect = EditorGUILayout.BeginVertical("box");
                        DrawOutline(rect, new Color(0.15f, 0.15f, 0.15f, 0.5f));
                        {
                            // Logger name and Is Enabled
                            EditorGUILayout.BeginHorizontal();
                            {
                                GUIHelper.PushColor(definitions[i].overridedColor);
                                EditorGUILayout.LabelField($"[{definitions[i].loggerName}]", LoggerNameTextGuiStyle);
                                GUIHelper.PopColor();

                                GUILayout.FlexibleSpace();

                                GUIHelper.PushGUIEnabled(!_refreshing && !Application.isPlaying);
                                EditorGUI.BeginChangeCheck();
                                definitions[i].isEnabled = EditorGUILayout.Toggle(definitions[i].isEnabled, GUILayout.Width(18f));
                                if (EditorGUI.EndChangeCheck())
                                {
                                    EditorUtility.SetDirty(InfoLoggerSettings.Instance);
                                    Helpers.Editor.SaveProject();
                                }
                                GUIHelper.PopGUIEnabled();
                            }
                            EditorGUILayout.EndHorizontal();

                            // Path
                            GUIHelper.PushColor(new Color(0.7f, 0.7f, 0.7f, 1f));
                            EditorGUILayout.LabelField($"Path: {definitions[i].path}", LoggerPathTextGuiStyle);
                            GUIHelper.PopColor();

                            // Color
                            EditorGUILayout.BeginHorizontal();
                            {
                                GUIHelper.PushLabelWidth(50f);
                                EditorGUI.BeginChangeCheck();
                                definitions[i].overridedColor = EditorGUILayout.ColorField("Color",
                                    definitions[i].overridedColor, GUILayout.Width(130f));
                                if (EditorGUI.EndChangeCheck())
                                {
                                    EditorUtility.SetDirty(InfoLoggerSettings.Instance);
                                }
                                GUIHelper.PopLabelWidth();

                                if (GUILayout.Button("Set Default", GUILayout.Width(80f), GUILayout.Height(18f)))
                                {
                                    definitions[i].overridedColor = definitions[i].defaultColor;
                                    EditorUtility.SetDirty(InfoLoggerSettings.Instance);
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            // Log filters
                            if (definitions[i].isEnabled)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUI.BeginChangeCheck();

                                    float toggleWidth = 80f;
                                    GUIHelper.PushLabelWidth(50f);

                                    GUIHelper.PushColor(Color.white);
                                    definitions[i].debugLogsEnabled =
                                        EditorGUILayout.ToggleLeft("Debug", definitions[i].debugLogsEnabled, GUILayout.Width(toggleWidth));
                                    GUIHelper.PopColor();

                                    GUIHelper.PushColor(Color.yellow);
                                    definitions[i].warningsLogsEnabled =
                                        EditorGUILayout.ToggleLeft("Warning", definitions[i].warningsLogsEnabled, GUILayout.Width(toggleWidth));
                                    GUIHelper.PopColor();

                                    GUIHelper.PushColor(Color.red);
                                    definitions[i].errorLogsEnabled =
                                        EditorGUILayout.ToggleLeft("Error", definitions[i].errorLogsEnabled, GUILayout.Width(toggleWidth));
                                    GUIHelper.PopColor();

                                    GUIHelper.PushColor(Color.red);
                                    definitions[i].exceptionLogsEnabled =
                                        EditorGUILayout.ToggleLeft("Exception", definitions[i].exceptionLogsEnabled, GUILayout.Width(toggleWidth));
                                    GUIHelper.PopColor();

                                    GUIHelper.PopLabelWidth();

                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        EditorUtility.SetDirty(InfoLoggerSettings.Instance);
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.Space();
                }

                EditorGUILayout.EndScrollView();
                // --- ScrollView End ---
            }

            if (_refreshing)
            {
                EditorGUI.LabelField(
                    new Rect(0f, 0f, position.width, position.height),
                    "WAITING FOR" + Environment.NewLine + "REFRESHING...",
                    RefreshingTextGuiStyle);
            }
        }

        private void Refresh(bool force)
        {
            _refreshing = true;
            _ = ScanProject().ContinueWith((taskResult) =>
            {
                List<(string path, string loggerName, Color color)> newList = taskResult;

                if (force)
                {
                    InfoLoggerSettings.Instance.definitions.Clear();
                }
                else
                {
                    var oldList = InfoLoggerSettings.Instance.definitions;

                    // Remove olds

                    List<InfoLoggerSettings.LoggerDefinition> definitionsWillRemove = new List<InfoLoggerSettings.LoggerDefinition>();
                    for (var i = 0; i < oldList.Count; i++)
                    {
                        var pathOld = oldList[i].path;
                        if (!newList.Any(x => x.path.Equals(pathOld, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            definitionsWillRemove.Add(oldList[i]);
                        }
                    }
                    for (var i = 0; i < definitionsWillRemove.Count; i++)
                    {
                        InfoLoggerSettings.Instance.definitions.Remove(definitionsWillRemove[i]);
                    }
                }

                // Add/Update news
                for (var i = 0; i < newList.Count; i++)
                {
                    string pathNew = newList[i].path;
                    InfoLoggerSettings.LoggerDefinition currentDefinition =
                        InfoLoggerSettings.Instance.definitions.FirstOrDefault(x =>
                            x.path.Equals(pathNew, StringComparison.InvariantCultureIgnoreCase));
                    if (currentDefinition == null)
                    {
                        InfoLoggerSettings.Instance.definitions.Add(new InfoLoggerSettings.LoggerDefinition()
                        {
                            path = newList[i].path,
                            loggerName = newList[i].loggerName,
                            isEnabled = true,
                            defaultColor = newList[i].color,
                            overridedColor = newList[i].color
                        });
                    }
                    else
                    {
                        // Update default color
                        currentDefinition.path = newList[i].path;
                        currentDefinition.loggerName = newList[i].loggerName;
                        currentDefinition.defaultColor = newList[i].color;
                    }
                }

                // Remove duplicates
                InfoLoggerSettings.Instance.definitions =
                    InfoLoggerSettings.Instance.definitions
                        .GroupBy(x => x.path).Select(y => y.First()).ToList();
                InfoLoggerSettings.Instance.definitions =
                    InfoLoggerSettings.Instance.definitions
                        .GroupBy(x => x.loggerName).Select(y => y.First()).ToList();

                // Order by
                InfoLoggerSettings.Instance.definitions = InfoLoggerSettings.Instance.definitions.OrderBy(x => x.loggerName).ToList();

                EditorUtility.SetDirty(InfoLoggerSettings.Instance);
                _refreshing = false;
            });
        }

        private void DrawOutline(Rect rect, Color color, float thickness = 1f)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            // Top
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            // Left
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            // Right
            EditorGUI.DrawRect(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color);
            // Bottom
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
        }


        private async UniTask<List<(string path, string loggerName, Color color)>> ScanProject()
        {
            var guids = AssetDatabase.FindAssets("t:Script");
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();

            await UniTask.SwitchToThreadPool();

            var regex = new Regex( @"(\bnew\s+InfoLogger|\bInfoLogger\s+\w+\s*=\s*new)\s*\((?<args>[^)]*)\)", RegexOptions.Compiled | RegexOptions.Singleline );
            
            var results = new ConcurrentBag<(string path, string loggerName, Color color)>();

            Parallel.ForEach(paths,
                path =>
                {
                    try
                    {
                        if (path.EndsWith("InfoLogger.cs", StringComparison.OrdinalIgnoreCase))
                        {
                            return;
                        }

                        string code = File.ReadAllText(path);
                        foreach (Match m in regex.Matches(code))
                        {
                            string val = m.Value;

                            if (val.Contains("\n") || val.Contains(System.Environment.NewLine) || val.Contains("\\n"))
                            {
                                Debug.LogError("Logger definition should be in single line: " + Environment.NewLine + $"{path} → {val}");
                                continue;
                            }

                            string args = m.Groups["args"].Value.Trim();                            
                            string loggerName = "";
                            string secondParam = "";

                            if (!string.IsNullOrEmpty(args))
                            {
                                // Split by commas only at top level (not inside quotes)
                                // Simple but works for your string params
                                var parts = args.Split(',')
                                    .Select(p => p.Trim())
                                    .ToArray();

                                if (parts.Length > 0) loggerName = parts[0];
                                if (parts.Length > 1) secondParam = parts[1];
                            }

                            loggerName = loggerName.Replace("\"", "");

                            if (string.IsNullOrEmpty(loggerName))
                            {
                                Debug.LogError("Logger definition has invalid logger name: " + Environment.NewLine + $"{path} → {val}");
                                continue;
                            }

                            secondParam = secondParam.Replace("\"", "").Trim();
                            Color color = Color.white;

                            if (string.IsNullOrEmpty(secondParam) || secondParam.ToLower() == "null")
                            {
                                color = Color.white;
                            }
                            else if (Enum.TryParse<ColorName>(secondParam, true, out var named))
                            {
                                // Handle unity's default colors like black, white, red etc...
                                switch (named)
                                {
                                    case ColorName.black: color = Color.black; break;
                                    case ColorName.white: color = Color.white; break;
                                    case ColorName.red: color = Color.red; break;
                                    case ColorName.green: color = Color.green; break;
                                    case ColorName.blue: color = Color.blue; break;
                                    case ColorName.yellow: color = Color.yellow; break;
                                    case ColorName.cyan: color = Color.cyan; break;
                                    case ColorName.magenta: color = Color.magenta; break;
                                    case ColorName.pink: color = Color.magenta; break;
                                    case ColorName.gray: color = Color.gray; break;
                                    case ColorName.grey: color = Color.grey; break;
                                    default: color = Color.white; break;
                                }
                            }
                            else
                            {
                                if (!secondParam.StartsWith("#"))
                                    secondParam = secondParam.Insert(0, "#");

                                if (!ColorUtility.TryParseHtmlString(secondParam, out color))
                                {
                                    // Can't parse
                                    Debug.LogError(
                                        "Logger definition has invalid color param, will use default one: " + Environment.NewLine + $"{path} → {val}");
                                    color = Color.white;
                                }
                            }

                            if (loggerName.Contains("nameof"))
                            {
                                Debug.LogError(
                                    "Logger definition including 'nameof' keyword, don't use it! " + Environment.NewLine + $"{path} → {val}");
                            }

                            path = path.Replace("\\", "/").Trim();
                            if (path.StartsWith("/"))
                                path = path.Substring(1);

                            results.Add((path, loggerName, color));
                        }
                    }
                    catch (Exception ex)
                    {
                        // optional: collect errors
                        Debug.LogError(path + Environment.NewLine + ex.Message);
                    }
                });

            var list = results.ToList();

            await UniTask.SwitchToMainThread();

            //Debug.LogError(results.ToArray().SerializeObject(true));

            return list;
        }

        private enum ColorName
        {
            black,
            white,
            red,
            green,
            blue,
            yellow,
            cyan,
            magenta,
            pink,
            gray,
            grey
        }
    }
}