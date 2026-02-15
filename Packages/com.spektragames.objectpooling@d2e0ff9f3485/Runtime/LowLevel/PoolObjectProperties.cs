using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    [System.Serializable]
    public class PoolObjectProperties
    {
        public int initialSize = 1;
        public int poolIncreaseSize = 1;
        public int maxPoolSize = 50;
        public PoolOptions poolOptions = PoolOptions.TriggerPoolCallbacks;
    }
}