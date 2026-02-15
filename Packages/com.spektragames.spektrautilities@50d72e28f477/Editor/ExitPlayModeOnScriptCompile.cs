using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Editor
{
    [InitializeOnLoad]
    public class ExitPlayModeOnScriptCompile
    {
        private static ExitPlayModeOnScriptCompile instance = null;

        static ExitPlayModeOnScriptCompile()
        {
            Unused(instance);
            instance = new ExitPlayModeOnScriptCompile();
        }

        private ExitPlayModeOnScriptCompile()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        ~ExitPlayModeOnScriptCompile()
        {
            EditorApplication.update -= OnEditorUpdate;
            instance = null;
        }

        private static void OnEditorUpdate()
        {
            if (EditorApplication.isPlaying && EditorApplication.isCompiling)
            {
                //Debug.Log("Exiting play mode due to script compilation.");
                EditorApplication.isPlaying = false;
            }
        }

        private static void Unused<T>(T unusedVariable)
        {
        }
    }
}