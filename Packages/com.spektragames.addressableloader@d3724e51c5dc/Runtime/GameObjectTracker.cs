using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Sirenix.OdinInspector;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace SpektraGames.AddressableLoader.Runtime
{
    [DisallowMultipleComponent]
    public class GameObjectTracker : MonoBehaviour
    {
        [ShowInInspector] [ReadOnly] private bool _destroyed = false;
        public bool Destroyed => _destroyed;
        
        [ShowInInspector] [ReadOnly] private bool _disabled = true;
        public bool Disabled => _disabled;

        public Action onDisabled;
        public Action onDestroyed;
        
        private void Awake()
        {
            
        }

        private void Start()
        {
        }

        private void OnEnable()
        {
            _disabled = false;
        }

        private void OnDisable()
        {
            _disabled = true;
            
            onDisabled?.Invoke();
        }

        private void OnDestroy()
        {
            _destroyed = true;
            
            onDestroyed?.Invoke();
        }
    }
}