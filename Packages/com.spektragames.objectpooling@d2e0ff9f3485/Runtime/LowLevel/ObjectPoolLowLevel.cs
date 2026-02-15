using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    public class ObjectPoolLowLevel : MonoBehaviour
    {
        public const PoolOptions DefaultPoolOptions = PoolOptions.TriggerPoolCallbacks;

        public Action OnPoolUpdated;

        private bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        public int Size => _poolQueue?.Count ?? 0;

        private PoolOptions _poolOptions;
        private GameObject _mainPoolObject;
        private int _poolId;
        private int _initialSize;
        private int _poolIncreaseSize;
        private int _maxPoolSize;

        protected Queue<GameObject> _poolQueue;
        protected Dictionary<GameObject, List<IPoolCallbacks>> _activeObjectDictionary;

        protected virtual void OnEnable()
        {
            OnPoolUpdated += LimitMaxSize;
        }

        protected virtual void OnDisable()
        {
            OnPoolUpdated -= LimitMaxSize;
        }

        protected virtual void OnDestroy()
        {
            if (IndependentPoolManager.HasPool(_poolId, out ObjectPoolLowLevel objectPoolLowLevel))
            {
                IndependentPoolManager.Logger.LogWarning($"The pool manager(ObjectPoolLowLevel) with id {_poolId} destroyed. Force deregistering.");
                IndependentPoolManager.DeregisterObjectPool(_poolId);
            }
        }

        public ObjectPoolLowLevel Initialize(
            int initialSize,
            int poolIncreaseSize,
            int maxPoolSize,
            PoolOptions poolOptions,
            GameObject poolObject,
            int poolId)
        {
            _poolQueue = new();
            _activeObjectDictionary = new();
            _isInitialized = true;

            this._poolId = poolId;
            this._initialSize = initialSize;
            this._poolIncreaseSize = poolIncreaseSize;
            this._maxPoolSize = maxPoolSize;
            this._mainPoolObject = poolObject;
            this._poolOptions = poolOptions;

            InstantiatePoolItem(initialSize);

            return this;
        }

        public GameObject Get()
        {
            return Get(Vector3.zero, Quaternion.identity);
        }

        public GameObject Get(Transform parent)
        {
            var poolObject = Get(Vector3.zero, Quaternion.identity, parent);
            poolObject.transform.localPosition = Vector3.zero;
            return poolObject;
        }

        public GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (_poolQueue.Count <= 0)
            {
                InstantiatePoolItem(_poolIncreaseSize);
            }

            var poolItem = _poolQueue.Dequeue();
            poolItem.transform.SetParent(parent);

            if (_poolOptions.HasFlag(PoolOptions.TriggerPoolCallbacks))
                TriggerInitializeCallbacks(poolItem);

            poolItem.SetActive(true);

            OnPoolUpdated?.Invoke();

            return poolItem;
        }

        public virtual void Return(GameObject poolItem)
        {
            if (poolItem.transform.parent == transform)
            {
                Debug.LogError($"{poolItem.name} is already in the pool", poolItem);
                return;
            }

            _poolQueue.Enqueue(poolItem);

            if (_poolOptions.HasFlag(PoolOptions.TriggerPoolCallbacks))
                TriggerResetCallbacks(poolItem);

            poolItem.SetActive(false);
            poolItem.transform.SetParent(transform);

            OnPoolUpdated?.Invoke();
        }

        public virtual void Clear()
        {
            while (Size > 0)
            {
                var poolItem = _poolQueue.Dequeue();
                // if (!_activeObjectDictionary.Remove(poolItem, out _))
                // {
                //     Debug.LogError($"{poolItem.name} in queue is not referenced in active objects");
                // }
                if (poolItem != null)
                    Destroy(poolItem);
            }

            if (_activeObjectDictionary.Count > 0)
            {
                // Debug.LogError($"There are {_activeObjectDictionary.Count} objects that were in activeObjectDictionary but not in pool queue. Initiating force delete. Pool: {gameObject.name}");
                foreach (var keyValuePair in _activeObjectDictionary)
                {
                    if (keyValuePair.Key != null)
                        Destroy(keyValuePair.Key);
                }
            }

            _activeObjectDictionary.Clear();

            OnPoolUpdated?.Invoke();
        }

        protected virtual void InstantiatePoolItem(int count = 0)
        {
            if (!IsInitialized)
            {
                Debug.LogError("Pool is not initialized yet");
                return;
            }

            if (count <= 0)
                count = 1;
            for (var i = 0; i < count; i++)
            {
                var poolObject = Instantiate(_mainPoolObject, transform);
                _poolQueue.Enqueue(poolObject);
                CachePoolObject(poolObject);
                poolObject.SetActive(false);
            }

            OnPoolUpdated?.Invoke();
        }

        public virtual void CachePoolObject(GameObject poolObject)
        {
            var callbacks = new List<IPoolCallbacks>();
            if (_poolOptions.HasFlag(PoolOptions.TriggerPoolCallbacks))
                callbacks.AddRange(poolObject.GetComponents<IPoolCallbacks>());
            _activeObjectDictionary.Add(poolObject, callbacks);
        }

        protected virtual void TriggerResetCallbacks(GameObject poolItem)
        {
            if (!_poolOptions.HasFlag(PoolOptions.TriggerPoolCallbacks))
                return;

            if (_activeObjectDictionary.TryGetValue(poolItem, out List<IPoolCallbacks> callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback.OnReturnToPool();
                }
            }
        }

        protected virtual void TriggerInitializeCallbacks(GameObject poolItem)
        {
            if (!_poolOptions.HasFlag(PoolOptions.TriggerPoolCallbacks))
                return;

            var callbacks = _activeObjectDictionary[poolItem];
            if (callbacks == null)
                return;

            foreach (var callback in callbacks)
            {
                callback.OnGetFromPool();
            }
        }

        protected virtual void LimitMaxSize()
        {
            if (_maxPoolSize <= 0)
                return;

            while (Size > _maxPoolSize)
            {
                var poolItem = _poolQueue.Dequeue();
                if (!_activeObjectDictionary.Remove(poolItem, out _))
                {
                    Debug.LogError($"{poolItem.name} in queue is not referenced in active objects");
                }

                Debug.LogError($"Max size reached. Destroying {poolItem.name}");
                Destroy(poolItem);
            }
        }
    }
}