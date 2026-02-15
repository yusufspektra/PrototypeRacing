using System;
using UnityEditor;

namespace CapyTools.Common.Editor
{
    public class GUIIndentScope : IDisposable
    {
        public GUIIndentScope()
        {
            EditorGUI.indentLevel++;
        }
        public void Dispose()
        {
            EditorGUI.indentLevel--;
        }
    }
}