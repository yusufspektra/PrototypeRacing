using System.Threading;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public class SingletonComponent<T> : MonoBehaviour where T : Component
    {
        [SerializeField, FoldoutGroup("Singleton"), LabelText("Dont Destroy On Load")]
        protected internal bool isDontDestroyOnLoad = false;

#if UNITY_EDITOR
        [ShowInInspector, ReadOnly, FoldoutGroup("Singleton")]
#endif
        private static T instance;

        private static readonly Thread MainThread = Thread.CurrentThread;

        public static T Instance
        {
            get
            {
                if (Thread.CurrentThread == MainThread &&
                    Application.isPlaying &&
                    instance == null &&
                    typeof(T).IsSubclassOf(typeof(SingletonResourcesPrefabComponent<T>)))
                {
                    string name = typeof(T).Name;
                    var prefab = Resources.Load<T>(name);

                    if (prefab != null)
                    {
                        T newInstance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                        (newInstance as SingletonComponent<T>)?.SetInstance();
                    }
                }

                return instance;
            }
            set => instance = value;
        }

        protected virtual void Awake()
        {
            SetInstance();
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public static bool Exists()
        {
            return instance != null;
        }

        public bool SetInstance()
        {
            if (instance != null && instance != gameObject.GetComponent<T>())
            {
                Debug.LogError("[SingletonComponent] Instance already set for type " + typeof(T));

                if (isDontDestroyOnLoad)
                    Destroy(gameObject);

                return false;
            }

            instance = gameObject.GetComponent<T>();

            if (isDontDestroyOnLoad)
            {
                gameObject.transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }

            return true;
        }
    }
}