using Sirenix.OdinInspector;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    public class PoolItem : ScriptableObject
    {
        public PoolEnum type;
        [AssetsOnly]
        [PreviewField(100f, ObjectFieldAlignment.Right)]
        public GameObject prefab;
        [Title("Spawning")]
        [MinValue(0)]
        public int initialPoolSize;
        [MinValue(1)]
        public int poolIncreaseSize = 1;
        [Tooltip("Max items a pool can hold. 0 represents no limit.")]
        [MinValue(0)]
        public int maxPoolSize;
        [Title("Callbacks")]
        public bool activatePoolCallbacks;
    }
}
