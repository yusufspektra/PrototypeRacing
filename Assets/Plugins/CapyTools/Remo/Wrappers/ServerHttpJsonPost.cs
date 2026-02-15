using System.Reflection;
using UnityEngine;

namespace CapyTools.RemoteEditor.Wrappers
{
    [AddComponentMenu("CapyTools/ServerHttpJsonPost")]
    public class ServerHttpJsonPost : Server.ServerHttpJsonPost
    {
        private static MethodInfo baseStart;
        
        private new void Start()
        {
            // Custom condition
            bool shouldRunBase = ShouldRunStart();
            
            //Debug.LogError("RemoServer.CustomStart: " + shouldRunBase);

            if (shouldRunBase)
            {
                Debug.Log("ServerHttpJsonPost[] base Start() calling");
                
                // Lazily cache the base Start() MethodInfo
                baseStart ??= typeof(RemoteEditor.RemoServer)
                    .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);

                // Invoke the original Start
                baseStart?.Invoke(this, null);
            }
            else
            {
                Debug.Log("ServerHttpJsonPost[] Skipped base Start()");
            }
        }

        private bool ShouldRunStart()
        {
            return !Application.isEditor;
        }
    }
}
