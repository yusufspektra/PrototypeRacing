using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    public class BasicObjectPool : ObjectPoolLowLevel
    {
        [SerializeField]
        private PoolEnum _poolEnum;
        public PoolEnum PoolEnum => _poolEnum;

        [SerializeField]
        private PoolItem _poolItem;
        public PoolItem PoolItem => _poolItem;

        public void Initialize(PoolEnum poolEnum)
        {
            _poolItem = PoolContainer.Instance.poolObjectDictionary[poolEnum];
            _poolEnum = _poolItem.type;

            base.Initialize(
                initialSize: _poolItem.initialPoolSize,
                poolIncreaseSize: _poolItem.poolIncreaseSize,
                maxPoolSize: _poolItem.maxPoolSize,
                poolOptions: _poolItem.activatePoolCallbacks ? PoolOptions.TriggerPoolCallbacks : PoolOptions.None,
                poolObject: _poolItem.prefab,
                poolId: PoolEnum.GetHashCode()
            );
        }
    }
}