using Sirenix.OdinInspector;
using SpektraGames.SpektraUtilities.Runtime;
using UniLabs.Time;
using UnityEngine;

namespace SpektraGames.RuntimeUI.Runtime
{
    [System.Serializable]
    public class UICallStacktraceInfo
    {
        public string status;
        public UDateTime dateTime;
        [HideInInspector] public System.Diagnostics.StackTrace stacktrace = null;
        [TextArea(7, 7)] public string stacktraceString = "";

        [Button]
        private void PrintToConsole()
        {
            Debug.Log(stacktrace.StacktraceToLog());
        }
    }
}