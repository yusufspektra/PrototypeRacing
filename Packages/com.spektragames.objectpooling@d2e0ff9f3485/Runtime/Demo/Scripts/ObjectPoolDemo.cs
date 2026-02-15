using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    public class ObjectPoolDemo : MonoBehaviour
    {
        public PoolEnum poolEnum;

        [Button]
        public void Get()
        {
            var go = PoolManager.Get(poolEnum);
            // Debug.LogError("Object fetched from pool", go);
        }

        [Button]
        public void Return(GameObject go)
        {
            PoolManager.Return(go, poolEnum);
            // Debug.LogError("Object returned to pool", go);
        }

        [Button]
        public void Clear()
        {
            PoolManager.Clear(poolEnum);
        }

        [Button]
        public void ClearCategory(string category)
        {
            PoolManager.ClearCategory(category);
        }

        [Button]
        public void ClearAll()
        {
            PoolManager.ClearAll();
            IndependentPoolManager.ClearAll();
        }

        [Button]
        public void IndependentRegister(int poolId, GameObject go)
        {
            IndependentPoolManager.RegisterObjectPool(
                "Test_" + UnityEngine.Random.Range(0, 10000).ToString(),
                poolId,
                go,
                new PoolObjectProperties()
                {
                });
        }

        [Button]
        public void IndependentDeregister(int poolId)
        {
            IndependentPoolManager.DeregisterObjectPool(poolId);
        }

        [Button]
        public void IndependentGet(int poolId)
        {
            IndependentPoolManager.Get(poolId);
        }

        [Button]
        public void IndependentReturn(int poolId, GameObject go)
        {
            IndependentPoolManager.Return(go, poolId);
        }
    }
}