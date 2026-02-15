using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    public interface IPoolCallbacks
    {
        void OnGetFromPool();
        
        void OnReturnToPool();
    }
}
