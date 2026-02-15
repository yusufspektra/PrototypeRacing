using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public class SingletonScriptableObject<T> : SingletonScriptableObjectBase where T : ScriptableObject
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    string name = typeof(T).Name;
                    instance = Resources.Load(name, typeof(T)) as T;
                }

                return instance;
            }
        }

        public static bool Exists()
        {
            return instance != null;
        }

        public static void Unload()
        {
            if (instance != null)
            {
                var so = instance as ScriptableObject;
                instance = null;
                
                try
                {
                    Resources.UnloadAsset(so);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }
}