using System;
using UnityEditor;
using UnityEngine;

namespace CapyTools.Common.Editor
{
    public class GUIIconSizeScope : IDisposable
    {
        public Vector2 _prev;
        public GUIIconSizeScope(Vector2 newValue)
        {
            _prev = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(newValue);
        }

        public void Dispose()
        {
            EditorGUIUtility.SetIconSize(_prev);
        }
    }
}