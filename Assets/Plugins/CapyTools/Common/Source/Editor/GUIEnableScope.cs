using System;
using UnityEngine;

namespace CapyTools.Common.Editor
{
    public class GUIEnableScope : IDisposable
    {
        private bool _prev;
        public GUIEnableScope(bool enable)
        {
            _prev = GUI.enabled;
            GUI.enabled = enable;
        }
        public void Dispose()
        {
            GUI.enabled = _prev;
        }
    }
}