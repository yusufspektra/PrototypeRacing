using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

namespace SpektraGames.SpektraUtilities.Runtime
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static Thread MainThread = null;
        private static readonly Queue<Action> ExecutionQueue = new Queue<Action>();
        private static MainThreadDispatcher RuntimeInstance = null;

#if UNITY_EDITOR
        static MainThreadDispatcher()
        {
            try
            {
                if (!Application.isPlaying)
                    EditorApplication.update += OnEditorUpdate;
            }
            catch
            {
            }
        }

        private static void OnEditorUpdate()
        {
            if (MainThread == null)
                MainThread = Thread.CurrentThread;

            UpdateQueue();
        }
#endif

        private void Start()
        {
        }

        public static void InitForRuntime()
        {
            if (RuntimeInstance == null)
            {
                if (!Application.isPlaying)
                {
                    return;
                }

                var obj = new GameObject("MainThreadDispatcher");
                RuntimeInstance = obj.AddComponent<MainThreadDispatcher>();
                MainThread = Thread.CurrentThread;
                DontDestroyOnLoad(obj);
            }
        }

        public static bool AmIOnTheMainThreadNow()
        {
            return MainThread != null && MainThread == Thread.CurrentThread;
        }

        public static void Enqueue(Action action)
        {
            if (MainThread != null &&
                Thread.CurrentThread == MainThread)
            {
                action();
                return;
            }

            Enqueue(ActionWrapper(action));
        }

        private static void Enqueue(IEnumerator action)
        {
            lock (ExecutionQueue)
            {
                ExecutionQueue.Enqueue(() =>
                {
                    if (Application.isEditor && !Application.isPlaying)
                    {
#if UNITY_EDITOR
                        EditorCoroutineUtility.StartCoroutineOwnerless(action);
#endif
                    }
                    else
                    {
                        if (RuntimeInstance == null)
                            InitForRuntime();

                        RuntimeInstance.StartCoroutine(action);
                    }
                });
            }
        }

        private static IEnumerator ActionWrapper(Action action)
        {
            action();
            yield break;
        }

        private void Update()
        {
            UpdateQueue();
        }

        private static void UpdateQueue()
        {
            lock (ExecutionQueue)
            {
                while (ExecutionQueue.Count > 0)
                {
                    ExecutionQueue.Dequeue().Invoke();
                }
            }
        }
    }
}