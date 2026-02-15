using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SpektraGames.ObjectPooling.Runtime
{
    public static class PoolManager
    {
        private static PoolContainer _poolContainer => PoolContainer.Instance;
        private static Dictionary<PoolEnum, BasicObjectPool> _poolDictionary;
        private static GameObject _poolParent;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _poolDictionary = new();
            SceneManager.sceneLoaded += OnSceneLoaded;
            AppDomain.CurrentDomain.ProcessExit += Cleanup;
            InitializePools();
        }

        private static void Cleanup(object sender, EventArgs e)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CheckNullPools();
            MergeAllPools();
        }

        private static void InitializePools()
        {
            _poolParent = new GameObject();
            _poolParent.name = "PoolParent";

            foreach (var keyValuePair in _poolContainer.poolObjectDictionary)
            {
                if (keyValuePair.Value.initialPoolSize > 0)
                    InitializeObjectPool(keyValuePair.Key);
            }
        }

        public static GameObject Get(PoolEnum poolEnum)
        {
            if (_poolDictionary.TryGetValue(poolEnum, out var objectPool))
            {
                return objectPool.Get();
            }
            else
            {
                objectPool = InitializeObjectPool(poolEnum);
                return objectPool.Get();
            }
        }

        private static BasicObjectPool InitializeObjectPool(PoolEnum poolEnum)
        {
            if (poolEnum == null)
            {
                Debug.LogError("Pool enum to initialize is null");
                return null;
            }

            BasicObjectPool objectPool;
            var gameObject = new GameObject();
            gameObject.name = $"{poolEnum.UniqueIdentifier}_Pool";
            gameObject.transform.SetParent(_poolParent.transform);
            objectPool = gameObject.AddComponent<BasicObjectPool>();
            objectPool.Initialize(poolEnum);
            _poolDictionary[objectPool.PoolEnum] = objectPool;
            return objectPool;
        }

        private static void InitializeObjectPoolTest()
        {
            var objectPool = InitializeObjectPool(EnemyPool.Zombie);
        }

        public static void Return(GameObject poolObject, PoolEnum poolEnum)
        {
            if (_poolDictionary.TryGetValue(poolEnum, out var objectPool))
            {
                objectPool.Return(poolObject);
            }
            else
            {
                Debug.LogError(
                    $"Object pool not found for enum '{poolEnum.UniqueIdentifier}'. Destroying {poolObject.name}.");
                GameObject.Destroy(poolObject);
            }
        }

        public static void Clear(PoolEnum poolEnum)
        {
            if (!_poolDictionary.Remove(poolEnum, out var objectPool))
                return;
            objectPool.Clear();
            GameObject.Destroy(objectPool.gameObject);
        }

        public static void ClearCategory(string poolCategory)
        {
            var objectPoolsToClear = _poolDictionary.Where(o => o.Key.Category == poolCategory)
                .Select(o => o.Value).ToList();
            foreach (var objectPool in objectPoolsToClear)
            {
                Clear(objectPool.PoolEnum);
            }
        }

        public static void ClearAll()
        {
            var objectPoolsToClear = _poolDictionary.Values
                .Select(o => o).ToList();
            foreach (var objectPool in objectPoolsToClear)
            {
                Clear(objectPool.PoolEnum);
            }

            _poolDictionary.Clear();
        }

        private static void MergeAllPools()
        {
            var objectPoolGroup = Object.FindObjectsByType<BasicObjectPool>(FindObjectsSortMode.None)
                .Where(o => o.PoolEnum != null && o.PoolEnum.Category != null)
                .GroupBy(o => o.PoolEnum.UniqueIdentifier);

            var poolsToDestroy = new List<BasicObjectPool>();
            foreach (var objectPools in objectPoolGroup)
            {
                // Choose the first pool in each category group as the target pool
                var targetPool = objectPools.FirstOrDefault();
                if (targetPool == null)
                    continue;

                // Transfer the objects from the other pools in the same category to the target pool
                foreach (var objectPool in objectPools.Skip(1)) // Skip the first, it's the target pool
                {
                    TransferPool(objectPool, targetPool);
                    poolsToDestroy.Add(objectPool);
                    if (!_poolDictionary.TryGetValue(targetPool.PoolEnum, out var cachedPool) ||
                        cachedPool == null ||
                        !ReferenceEquals(targetPool, objectPool))
                    {
                        _poolDictionary[targetPool.PoolEnum] = targetPool;
                    }
                }
            }

            foreach (var pool in poolsToDestroy)
            {
                GameObject.Destroy(pool.gameObject);
            }
        }

        private static void TransferPool(BasicObjectPool fromPool, BasicObjectPool toPool)
        {
            if (fromPool.Size == 0)
            {
                return;
            }

            if (ReferenceEquals(fromPool, toPool))
            {
                Debug.LogError($"Object pools are the same - {fromPool.gameObject.name}, {toPool.gameObject.name}");
                return;
            }

            // Debug.LogError($"Transferring {fromPool.Size} elements from {fromPool.name} to {toPool.name}", toPool);

            while (fromPool.Size > 0)
            {
                var poolObject = fromPool.Get(null);
                toPool.Return(poolObject);
                toPool.CachePoolObject(poolObject);
            }

            GameObject.Destroy(fromPool.gameObject);
        }

        private static void CheckNullPools()
        {
            var keysToRemove = new List<PoolEnum>();

            foreach (var keyValuePair in _poolDictionary)
            {
                if (keyValuePair.Value == null)
                {
                    keysToRemove.Add(keyValuePair.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _poolDictionary.Remove(key);
            }
        }
    }
}