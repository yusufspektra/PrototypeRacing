using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpektraGames.AddressableLoader.Runtime
{
    [System.Serializable]
    public class AssetLoadParams
    {
        public AssetReleaseMethodType methodType;

        public bool useDelayForReleaseHandle = false;
        public float delay = -1f;

        // WhenGameObjectDestroyed
        // WhenGameObjectDisabled
        public GameObject objectToTrack;

        public AssetLoadParams()
        {
        }

        public AssetLoadParams(AssetReleaseMethodType methodType)
        {
            this.methodType = methodType;
        }
    }
}