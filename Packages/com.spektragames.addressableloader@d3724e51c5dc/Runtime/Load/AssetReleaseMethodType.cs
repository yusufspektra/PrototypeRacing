using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpektraGames.AddressableLoader.Runtime
{
    public enum AssetReleaseMethodType
    {
        // When you load an asset, a reference count will be increase for this asset.
        // This reference number will decrease as you release the asset.
        // If the reference count decreases to 0, the asset will release from memory completely.
        ByReferenceCount = 0,

        // The asset instance will release when specified game object destroyed.
        WhenGameObjectDestroyed = 1,

        // The asset instance will release when specified game object deactivated/disabled.
        // Also, it will work when game object destroyed.
        WhenGameObjectDisabled = 2,

        // The asset instance will release when active scene changed.
        WhenActiveSceneChanged = 3
    }
}