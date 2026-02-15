using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public class SingletonScriptableObjectBase : ScriptableObject
    {
        public static async UniTask UnloadAllWithReflection()
        {
            List<MethodInfo> methods = new List<MethodInfo>();

            await UniTask.SwitchToThreadPool();

            List<Type> singletonTypes = new List<Type>();
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (var i = 0; i < allAssemblies.Length; i++)
            {
                var allTypes = allAssemblies[i].GetTypes();

                singletonTypes.AddRange(allTypes
                    .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType &&
                                t.IsSubclassOf(typeof(SingletonScriptableObjectBase))));
            }

            for (var i = 0; i < singletonTypes.Count; i++)
            {
                var type = singletonTypes[i];
                var unloadMethod = type.GetMethod("Unload",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (unloadMethod != null)
                {
                    methods.Add(unloadMethod);
                    //Debug.Log($"{type.Name}.Unload() found");
                }
                else
                {
                    //Debug.LogError("unloadMethod is null for type: " + type.FullName);
                }
            }

            await UniTask.SwitchToMainThread();

            for (var i = 0; i < methods.Count; i++)
            {
                try
                {
                    methods[i].Invoke(null, null);
                    //Debug.Log($"{type.Name}.Unload() called successfully");
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }
    }
}