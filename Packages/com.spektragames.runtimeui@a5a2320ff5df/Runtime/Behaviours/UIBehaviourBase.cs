using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniLabs.Time;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SpektraGames.RuntimeUI.Runtime
{
    public class UIBehaviourBase<T> : MonoBehaviour where T : Behaviour
    {
        protected static readonly List<T> ActiveBehaviours = new List<T>();

        public virtual T GetBehaviour => null;

        [FoldoutGroup("Base", false)]
        [PropertyOrder(-1)]
        [ShowInInspector, ReadOnly]
        public bool IsActive { get; private set; } = false;

        [FoldoutGroup("Base", false)]
        public RectTransform content;

        [FoldoutGroup("Base", false)]
        public UnityEvent onContentActivating = new UnityEvent();
        [FoldoutGroup("Base", false)]
        public UnityEvent onContentDeactivating = new UnityEvent();

#if UNITY_EDITOR
        [FoldoutGroup("Base", false)]
        [ShowInInspector, NonSerialized]
        private bool _collectStacktraces = false;

        [ShowInInspector, System.NonSerialized, ShowIf(nameof(_collectStacktraces))]
        private List<UICallStacktraceInfo> _stacktraces = new List<UICallStacktraceInfo>();
#endif

        protected virtual void Awake()
        {
            content.gameObject.SetActive(false);
        }

        protected virtual void OnDestroy()
        {
        }

        public virtual void ActivateContent()
        {
            onContentActivating?.Invoke();
            content.gameObject.SetActive(true);
            IsActive = true;

            if (!ActiveBehaviours.Contains(GetBehaviour))
                ActiveBehaviours.Add(GetBehaviour);

#if UNITY_EDITOR
            if (_collectStacktraces)
            {
                AddStacktrace("OnActivate");
            }
#endif
        }

        public virtual void DeactivateContent()
        {
            onContentDeactivating?.Invoke();
            content.gameObject.SetActive(false);
            IsActive = false;

            if (ActiveBehaviours.Contains(GetBehaviour))
                ActiveBehaviours.Remove(GetBehaviour);

#if UNITY_EDITOR
            if (_collectStacktraces)
            {
                AddStacktrace("OnDeactivate");
            }
#endif
        }

#if UNITY_EDITOR
        private void AddStacktrace(string status)
        {
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace(true);
            string stacktraceString = t.ToString();

            _stacktraces.Insert(0, new UICallStacktraceInfo()
            {
                status = status,
                dateTime = new UDateTime(DateTime.Now),
                stacktrace = t,
                stacktraceString = stacktraceString
            });

            while (_stacktraces.Count > 50)
            {
                _stacktraces.RemoveAt(_stacktraces.Count - 1);
            }
        }
#endif
    }
}