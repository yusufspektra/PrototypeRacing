using Debug = UnityEngine.Debug;
using System.Diagnostics;
#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
#endif

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static partial class Helpers
    {
#if UNITY_EDITOR
        public static class Editor
        {
            public static void SetBuildSettingsScenes(EditorBuildSettingsScene[] scenes)
            {
                SetBuildSettingsScenes(scenes.ToList());
            }

            public static void SetBuildSettingsScenes(List<EditorBuildSettingsScene> scenes)
            {
                SaveProject();

                string editorBuildSettingsRelativePath = "ProjectSettings/EditorBuildSettings.asset";
                var editorBuildSettingsObject =
                    AssetDatabase.LoadAssetAtPath<EditorBuildSettings>(editorBuildSettingsRelativePath);

                if (editorBuildSettingsObject != null)
                {
                    var allLines =
                        System.IO.File.ReadAllLines(File.RelativePathToNormalPath(editorBuildSettingsRelativePath));
                    bool startToParseScenes = false;
                    List<Tuple<bool, string, string>>
                        currentScenes = new List<Tuple<bool, string, string>>(); // enabled, path, guid
                    int mScenesLineIndex = -1;
                    for (int i = 0; i < allLines.Length; i++)
                    {
                        if (!startToParseScenes && allLines[i].Contains("m_Scenes:"))
                        {
                            startToParseScenes = true;
                            mScenesLineIndex = i;
                            continue;
                        }

                        if (startToParseScenes)
                        {
                            if (allLines[i].Contains("- enabled:"))
                            {
                                string enabled = allLines[i + 0]
                                    .Substring(allLines[i + 0].IndexOf("- enabled:") + 10 + 1);
                                string path = allLines[i + 1].Substring(allLines[i + 1].IndexOf("path:") + 5 + 1);
                                string guid = allLines[i + 2].Substring(allLines[i + 2].IndexOf("guid:") + 5 + 1);
                                currentScenes.Add(new Tuple<bool, string, string>((enabled == "0" ? false : true), path,
                                    guid));
                                i += 2;
                            }
                            else if (!allLines[i].Contains("- enabled:") && !allLines[i].Contains("- path:") &&
                                     !allLines[i].Contains("- guid:"))
                            {
                                break;
                            }
                        }
                    }

                    if (mScenesLineIndex <= 0)
                    {
                        Debug.LogError("mScenesLineIndex is wrong");
                        return;
                    }

                    List<Tuple<bool, string, string>>
                        newScenes = new List<Tuple<bool, string, string>>(); // enabled, path, guid
                    for (int i = 0; i < scenes.Count; i++)
                    {
                        newScenes.Add(new Tuple<bool, string, string>(scenes[i].enabled, scenes[i].path,
                            scenes[i].guid.ToString()));
                    }

                    var allLinesList = allLines.ToList();
                    allLinesList.RemoveRange(mScenesLineIndex + 1, currentScenes.Count * 3);
                    List<string> addList = new List<string>();
                    for (int i = 0; i < newScenes.Count; i++)
                    {
                        addList.Add("  - enabled: " + (newScenes[i].Item1 ? "1" : "0"));
                        addList.Add("    path: " + (newScenes[i].Item2));
                        addList.Add("    guid: " + (newScenes[i].Item3));
                    }

                    allLinesList.InsertRange(mScenesLineIndex + 1, addList);
                    //Debug.LogError(allLines.SerializeObject(true));

                    File.WriteAllLinesWithoutAppendExtraLine(allLinesList,
                        File.RelativePathToNormalPath(editorBuildSettingsRelativePath));
                    //AssetDatabase.StopAssetEditing();
                    AssetDatabase.ImportAsset(editorBuildSettingsRelativePath, ImportAssetOptions.ForceUpdate);
                    SaveProject();
                    AssetDatabase.Refresh();
                    SaveProject();
                    EditorBuildSettings.scenes = scenes.ToArray();
                    AssetDatabase.Refresh();
                    SaveProject();
                }
                else
                {
                    Debug.LogError("editorBuildSettingsObject is null: " + editorBuildSettingsRelativePath);
                }
            }

            public static void SaveProject()
            {
                if (Application.isBatchMode)
                {
                    EditorSceneManager.SaveOpenScenes();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();   
                    return;
                }
                
                AssetDatabase.SaveAssets();
                SaveFromFileMenu();
                AssetDatabase.Refresh();
                SaveFromFileMenu();

                void SaveFromFileMenu()
                {
                    try
                    {
                        EditorApplication.ExecuteMenuItem("File/Save");
                        EditorApplication.ExecuteMenuItem("File/Save Project");
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            public static List<T> FindAssetsByTypeAtPath<T>(string relativePath) where T : UnityEngine.Object
            {
                ArrayList al = new ArrayList();
                string[] fileEntries =
                    Directory.GetFiles(File.RelativePathToNormalPath(relativePath), "*", SearchOption.AllDirectories);

                foreach (string fileName in fileEntries)
                {
                    string localPath = File.NormalPathToRelativePath(fileName);

                    UnityEngine.Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

                    if (t != null)
                        al.Add(t);
                }

                List<T> result = new List<T>();
                for (int i = 0; i < al.Count; i++)
                    result.Add((T)al[i]);

                return result;
            }

            public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
            {
                List<T> assets = new List<T>();
                string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
                for (int i = 0; i < guids.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    if (asset != null)
                    {
                        assets.Add(asset);
                    }
                }

                return assets;
            }

            public static List<object> FindAssetsByType(Type type)
            {
                List<object> assets = new();
                string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", type));
                for (int i = 0; i < guids.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
                    if (asset != null)
                    {
                        assets.Add(asset);
                    }
                }

                return assets;
            }

            public static void MoveFileInPlasticSCM(string oldPath, string newPath)
            {
                string oldPathForMetaName = oldPath + ".meta";
                string newPathForMetaName = newPath + ".meta";

                string cmdToWorkForFileName = $"cm move \"{oldPath}\" \"{newPath}\"";
                string cmdToWorkForMetaName = $"cm move \"{oldPathForMetaName}\" \"{newPathForMetaName}\"";

                if (Application.platform == RuntimePlatform.OSXEditor ||
                    Application.platform == RuntimePlatform.OSXPlayer ||
                    Application.platform == RuntimePlatform.OSXServer)
                {
                    cmdToWorkForFileName = "/usr/local/bin/" + cmdToWorkForFileName;
                    cmdToWorkForMetaName = "/usr/local/bin/" + cmdToWorkForMetaName;
                }

                //Debug.LogError(cmdToWorkForFileName);
                string resultForFileName = RunCommandInTerminal(cmdToWorkForFileName);
                string resultForMetaName = RunCommandInTerminal(cmdToWorkForMetaName);
            }

            public static string RunCommandInTerminal(string command, bool logging = true)
            {
                return RunCommandInTerminal(command, out _, logging);
            }

            public static string RunCommandInTerminal(string command, out string error, bool logging = true)
            {
                if (Application.platform == RuntimePlatform.OSXEditor ||
                    Application.platform == RuntimePlatform.OSXPlayer ||
                    Application.platform == RuntimePlatform.OSXServer)
                {
                    // Mac
                    try
                    {
                        if (logging)
                            UnityEngine.Debug.Log("============== Start Executing [" + command + "] ===============");
                        ProcessStartInfo startInfo = new ProcessStartInfo()
                        {
                            FileName = "/bin/bash",
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true,
                            Arguments = "-lc \"" + command + " \""
                        };
                        Process myProcess = new Process
                        {
                            StartInfo = startInfo
                        };
                        myProcess.Start();
                        string output = myProcess.StandardOutput.ReadToEnd();
                        string errorOutput = null;
                        if (output != null && logging)
                            UnityEngine.Debug.Log(output);
                        if (string.IsNullOrEmpty(output))
                        {
                            errorOutput = myProcess.StandardError.ReadToEnd();
                            if (!string.IsNullOrEmpty(errorOutput))
                                UnityEngine.Debug.LogError(errorOutput);
                        }

                        myProcess.WaitForExit();
                        if (logging)
                            UnityEngine.Debug.Log("============== End ===============");

                        error = errorOutput;
                        return output;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                        error = e.ToString();
                        return null;
                    }
                }
                else
                {
                    // Windows

                    try
                    {
                        if (logging)
                            UnityEngine.Debug.Log("============== Start Executing [" + command + "] ===============");
                        ProcessStartInfo startInfo = new ProcessStartInfo()
                        {
                            FileName = "cmd.exe",
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true,
                            Arguments = "/C " + command
                        };
                        Process myProcess = new Process
                        {
                            StartInfo = startInfo
                        };
                        myProcess.Start();
                        string output = myProcess.StandardOutput.ReadToEnd();
                        string errorOutput = null;
                        if (output != null && logging)
                            UnityEngine.Debug.Log(output);
                        if (string.IsNullOrEmpty(output))
                        {
                            errorOutput = myProcess.StandardError.ReadToEnd();
                            if (!string.IsNullOrEmpty(errorOutput))
                                UnityEngine.Debug.LogError(errorOutput);
                        }

                        myProcess.WaitForExit();
                        if (logging)
                            UnityEngine.Debug.Log("============== End ===============");

                        error = errorOutput;
                        return output;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                        error = e.ToString();
                        return null;
                    }
                }
            }

            public static void ClearEditorConsole()
            {
                Debug.ClearDeveloperConsole();
                var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
                var type = assembly.GetType("UnityEditor.LogEntries");
                var method = type.GetMethod("Clear");
                method.Invoke(new object(), null);
            }

            public static void RefreshEditor()
            {
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }

            public static void ForceRecompile()
            {
#if UNITY_EDITOR
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
#endif
            }
        }
#endif
    }
}