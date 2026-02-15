using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class CSCHelperForEditor
    {
        private const string CSCPathRelative = "Assets/csc.rsp";

        public static string CSCPath
        {
            get
            {
                string projectPath =
                    Application.dataPath.Replace("\\", "/").Replace("/Assets", "/");
                return Path.Combine(projectPath, CSCPathRelative);
            }
        }

        private static List<string> DefaultAssembliesToAddCSC = new List<string>()
        {
            "-r:System.IO.Compression.dll",
            "-r:System.IO.Compression.FileSystem.dll"
        };

        public static void TryAddAndRemoveSymbols(List<string> symbolsToAdd, List<string> symbolsToRemove)
        {
            if (symbolsToAdd != null)
            {
                for (var i = 0; i < symbolsToAdd.Count; i++)
                {
                    TryAddSymbol(symbolsToAdd[i]);
                }
            }

            if (symbolsToRemove != null)
            {
                for (var i = 0; i < symbolsToRemove.Count; i++)
                {
                    TryRemoveSymbol(symbolsToRemove[i]);
                }
            }
        }

        public static void TryAddSymbols(List<string> symbols)
        {
            for (int i = 0; i < symbols.Count; i++)
            {
                TryAddSymbol(symbols[i]);
            }
        }

        public static bool TryAddSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                Debug.LogError("[CSCHelperForEditor] You can't add null or empty symbol");
                return false;
            }

            if (symbol.Contains("define:"))
            {
                Debug.LogError("[CSCHelperForEditor] The symbol you want to add shouldn't contains '-define:' key");
                return false;
            }

            var currentSymbols = GetCurrentSymbols();
            if (!currentSymbols.Contains(symbol.Trim()))
            {
                currentSymbols.Add(symbol.Trim());
                WriteSymbols(currentSymbols);
                Debug.Log("[CSCHelperForEditor] Try add symbols success for " + symbol);
                return true;
            }

            Debug.Log("[CSCHelperForEditor] Try add symbols success for " + symbol);

            return false;
        }

        public static void TryRemoveSymbols(List<string> symbols)
        {
            for (int i = 0; i < symbols.Count; i++)
            {
                TryRemoveSymbol(symbols[i]);
            }
        }

        public static bool TryRemoveSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                Debug.LogError("[CSCHelperForEditor] You can't remove null or empty symbol");
                return false;
            }

            if (symbol.Contains("define:"))
            {
                Debug.LogError("[CSCHelperForEditor] The symbol you want to remove shouldn't contains '-define:' key");
                return false;
            }

            var currentSymbols = GetCurrentSymbols();
            if (currentSymbols.Contains(symbol.Trim()))
            {
                currentSymbols.RemoveAll(x => x.Contains(symbol.Trim()));
                WriteSymbols(currentSymbols);
                Debug.Log("[CSCHelperForEditor] Try remove symbols success for " + symbol);
                return true;
            }

            Debug.Log("[CSCHelperForEditor] Try remove symbols failed for " + symbol);

            return false;
        }

        public static void RemoveAllSymbols()
        {
            Debug.Log("[CSCHelperForEditor] RemoveAllSymbols");

            var currentSymbols = GetCurrentSymbols();

            if (!currentSymbols.IsNullOrEmpty())
            {
                currentSymbols.Clear();
                WriteSymbols(currentSymbols);
            }
        }

        public static bool HasSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                Debug.LogError("[CSCHelperForEditor] You can't check null or empty symbol");
                return false;
            }

            if (symbol.Contains("define:"))
            {
                Debug.LogError("[CSCHelperForEditor] The symbol you want to check shouldn't contains '-define:' key");
                return false;
            }

            var currentSymbols = GetCurrentSymbols();
            return currentSymbols.Contains(symbol.Trim());
        }

        public static List<string> GetCurrentSymbols()
        {
            string cscPath = CSCPath;

            if (!File.Exists(cscPath))
            {
                Helpers.File.WriteAllLinesWithoutAppendExtraLine(DefaultAssembliesToAddCSC, cscPath);
                Debug.LogError("[CSCHelperForEditor] CSC file not found in this path. Created one: " + cscPath);
            }

            var allLines = Helpers.File.ReadAllLinesWithoutAppendExtraLine(cscPath);
            List<string> symbols = new List<string>();
            for (int i = 0; i < allLines.Count; i++)
            {
                if (allLines[i].StartsWith("-define:"))
                {
                    symbols.Add(allLines[i].Replace("-define:", "").Trim());
                }
            }

            return symbols;
        }

        public static void RefreshEditor()
        {
#if UNITY_EDITOR
            Helpers.Editor.RefreshEditor();
#endif
        }

        public static void ForceRecompile()
        {
#if UNITY_EDITOR
            Helpers.Editor.ForceRecompile();
#endif
        }

        private static void WriteSymbols(List<string> allSymbols)
        {
            string cscPath = CSCPath;

            List<string> fileLines = new List<string>();

            fileLines.AddRange(DefaultAssembliesToAddCSC);

            for (var i = 0; i < allSymbols.Count; i++)
            {
                if (!string.IsNullOrEmpty(allSymbols[i]))
                {
                    fileLines.Add("-define:" + allSymbols[i].Trim());
                }
            }

            Helpers.File.WriteAllLinesWithoutAppendExtraLine(fileLines, cscPath);

            Debug.Log("[CSCHelperForEditor] WriteSymbols: " + allSymbols.SerializeObject(true));
        }
    }
}