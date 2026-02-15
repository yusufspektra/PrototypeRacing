using System.Reflection;
using UnityEngine;

namespace CapyTools.RemoteEditor.Wrappers
{
    [AddComponentMenu("CapyTools/Remo Server")]
    public class RemoServer : RemoteEditor.RemoServer
    {
        private static MethodInfo baseAwake;
        private static FieldInfo fieldUsePlayerConnection;
        private static FieldInfo fieldUseHttpConnection;

        private new void Awake()
        {
            // Custom condition
            bool shouldRunBase = ShouldRunAwake();
            
            //Debug.LogError("RemoServer.CustomAwake: " + shouldRunBase);

            if (shouldRunBase)
            {
                Debug.Log("RemoServer[] base Awake() calling");
                
                // Lazily cache the base Awake() MethodInfo
                baseAwake ??= typeof(RemoteEditor.RemoServer)
                    .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);

                // Invoke the original Awake
                baseAwake?.Invoke(this, null);
            }
            else
            {
                Debug.Log("RemoServer[] Skipped base Awake()");
                SetPrivateField("usePlayerConnection", false);
                SetPrivateField("useHttpConnection", false);
            }
        }

        private void SetPrivateField(string fieldName, bool value)
        {
            // Cache FieldInfos for efficiency
            if (fieldName == "usePlayerConnection")
                (fieldUsePlayerConnection ??= typeof(RemoteEditor.RemoServer)
                        .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)).SetValue(this, value);

            else if (fieldName == "useHttpConnection")
                (fieldUseHttpConnection ??= typeof(RemoteEditor.RemoServer)
                        .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)).SetValue(this, value);
        }


        private bool ShouldRunAwake()
        {
            return !Application.isEditor;
        }
    }
}