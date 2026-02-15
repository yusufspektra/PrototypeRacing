using System.Collections;
using System.Collections.Generic;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    public static class IndependentPoolManager
    {
        private static Dictionary<int, ObjectPoolLowLevel> _poolDictionary = new Dictionary<int, ObjectPoolLowLevel>();
        private static Transform _poolParent;
        private static Transform PoolParent
        {
            get
            {
                if (!_poolParent)
                {
                    _poolParent = new GameObject("_IndependentPoolParent").transform;
                }

                return _poolParent;
            }
        }

        internal static readonly InfoLogger Logger = new InfoLogger("IndependentPoolManager", "#ff0059");

        public static void ClearAll()
        {
            Logger.Log("Clear All");

            foreach (var objectPoolLowLevel in _poolDictionary)
            {
                if (objectPoolLowLevel.Value)
                {
                    objectPoolLowLevel.Value.Clear();
                    GameObject.Destroy(objectPoolLowLevel.Value);
                }
            }

            _poolDictionary = new Dictionary<int, ObjectPoolLowLevel>();

            if (_poolParent)
                GameObject.Destroy(_poolParent);
        }

        public static bool HasPool(int poolId, out ObjectPoolLowLevel pool)
        {
            if (_poolDictionary.TryGetValue(poolId, out pool))
            {
                return true;
            }

            pool = null;
            return false;
        }

        public static void RegisterObjectPool(
            string poolName,
            int poolId,
            GameObject gameObject,
            PoolObjectProperties properties,
            Transform specificPoolParent = null)
        {
            if (!gameObject)
            {
                Logger.LogError("gameObject null");
                return;
            }

            if (_poolDictionary.TryGetValue(poolId, out ObjectPoolLowLevel objectPool))
            {
                Logger.LogError("_poolDictionary already have this id: " + poolId);
                return;
            }

            if (properties == null)
                properties = new PoolObjectProperties();

            _poolDictionary[poolId] =
                new GameObject(poolName).AddComponent<ObjectPoolLowLevel>()
                    .Initialize(
                        properties.initialSize,
                        properties.poolIncreaseSize,
                        properties.maxPoolSize,
                        properties.poolOptions,
                        gameObject,
                        poolId
                    );

            if (specificPoolParent)
            {
                _poolDictionary[poolId].transform.SetParent(specificPoolParent);
            }
            else
            {
                _poolDictionary[poolId].transform.SetParent(PoolParent);
            }

            Logger.Log($"RegisterObjectPool:: PoolName: {poolName}, PoolId: {poolId}");
        }

        public static void DeregisterObjectPool(int poolId)
        {
            if (!_poolDictionary.TryGetValue(poolId, out ObjectPoolLowLevel objectPool))
            {
                Logger.LogError("_poolDictionary don't have this id: " + poolId);
                return;
            }

            if (objectPool)
            {
                objectPool.Clear();
                GameObject.Destroy(objectPool);
            }

            _poolDictionary.Remove(poolId);
            Logger.Log($"DeregisterObjectPool:: PoolId: {poolId}");
        }

        public static GameObject Get(int poolId)
        {
            return Get(poolId, Vector3.zero, Quaternion.identity, null);
        }

        public static GameObject Get(int poolId, Transform parent)
        {
            return Get(poolId, Vector3.zero, Quaternion.identity, parent);
        }

        public static GameObject Get(int poolId, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!_poolDictionary.TryGetValue(poolId, out ObjectPoolLowLevel objectPool))
            {
                Logger.LogError("_poolDictionary don't have this id: " + poolId);
                return null;
            }

            if (!objectPool)
            {
                Logger.LogError("objectPool is null for this pool id: " + poolId);
                return null;
            }

            return objectPool.Get(position, rotation, parent);
        }

        public static T GetByComponent<T>(int poolId) where T : Component
        {
            return GetByComponent<T>(poolId, Vector3.zero, Quaternion.identity, null);
        }

        public static T GetByComponent<T>(int poolId, Transform parent) where T : Component
        {
            return GetByComponent<T>(poolId, Vector3.zero, Quaternion.identity, parent);
        }

        public static T GetByComponent<T>(int poolId, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component
        {
            GameObject obj = Get(poolId, position, rotation, parent);

            if (obj)
            {
                if (obj.TryGetComponent(out T component))
                {
                    return component;
                }
                else
                {
                    Logger.LogError($"Component not found on {obj.name} for {nameof(T)} component. Will return to pool.",
                        obj);
                    Return(obj, poolId);
                    return null;
                }
            }

            return null;
        }

        public static void Return(GameObject poolObject, int poolId)
        {
            if (!_poolDictionary.TryGetValue(poolId, out ObjectPoolLowLevel objectPool))
            {
                Logger.LogError("_poolDictionary don't have this id: " + poolId);
                return;
            }

            if (!objectPool)
            {
                Logger.LogError("objectPool is null for this pool id: " + poolId);
                return;
            }

            objectPool.Return(poolObject);
        }
    }
}