using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static partial class Helpers
    {
        public static class File
        {
            public static void WriteAllLinesWithoutAppendExtraLine(List<string> lines, string path)
            {
                StringBuilder sb = new StringBuilder();
                if (lines != null)
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        sb.Append(lines[i]);
                        if (i != lines.Count - 1)
                        {
                            sb.Append(System.Environment.NewLine);
                        }
                    }
                }

                System.IO.File.WriteAllText(path, sb.ToString());
            }

            public static void WriteAllLinesWithoutAppendExtraLine(string[] lines, string path)
            {
                StringBuilder sb = new StringBuilder();
                if (lines != null)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        sb.Append(lines[i]);
                        if (i != lines.Length - 1)
                        {
                            sb.Append(System.Environment.NewLine);
                        }
                    }
                }

                System.IO.File.WriteAllText(path, sb.ToString());
            }

            public static List<string> ReadAllLinesWithoutAppendExtraLine(string path)
            {
                List<string> allLines = System.IO.File.ReadAllLines(path).ToList();
                if (allLines.Count >= 1 &&
                    string.IsNullOrEmpty(allLines[allLines.Count - 1].Trim()))
                {
                    allLines.RemoveAt(allLines.Count - 1);
                }

                return allLines;
            }

            public static void EmptyDirectory(string path)
            {
                string[] files = Directory.GetFiles(path);
                string[] dirs = Directory.GetDirectories(path);

                foreach (string file in files)
                {
                    System.IO.File.SetAttributes(file, FileAttributes.Normal);
                    System.IO.File.Delete(file);
                }

                foreach (string dir in dirs)
                {
                    EmptyDirectory(dir);
                }
            }

            public static void DeleteDirectory(string path)
            {
                EmptyDirectory(path);
                Directory.Delete(path);
            }

            public static string RelativePathToNormalPath(string relativePath)
            {
                string projectPath =
                    Application.dataPath.Replace("\\", "/").Replace("/Assets", "/");
                return (projectPath + relativePath).Replace("\\", "/");
            }

            public static string NormalPathToRelativePath(string normalPath)
            {
                string projectPath =
                    Application.dataPath.Replace("\\", "/").Replace("/Assets", "/");
                return normalPath.Replace("\\", "/").Replace(projectPath, "");
            }

            public static void CreateDirectoriesIfNotExistOnPath(string path)
            {
                path = path.Replace("\\", "/");
                var directories = path.Split('/').ToList();

                if (directories != null && directories.Count > 0)
                {
                    if (Path.HasExtension(path))
                        directories.RemoveAt(directories.Count - 1);

                    path = "";
                    for (int i = 0; i < directories.Count; i++)
                    {
                        path += directories[i] + "/";
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                    }
                }
            }
        }
    }
}