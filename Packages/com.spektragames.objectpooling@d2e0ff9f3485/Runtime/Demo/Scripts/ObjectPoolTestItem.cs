using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    public class ObjectPoolTestItem : MonoBehaviour, IPoolCallbacks
    {
        public void OnReturnToPool()
        {
            Debug.LogError("OnPoolObjectEnqueued", gameObject);
        }

        public void OnGetFromPool()
        {
            Debug.LogError("OnPoolObjectDequeued", gameObject);
        }
    }
}
