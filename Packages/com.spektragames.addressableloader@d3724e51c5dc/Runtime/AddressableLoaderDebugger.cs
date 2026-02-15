using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

namespace SpektraGames.AddressableLoader.Runtime
{
    public class AddressableLoaderDebugger : MonoBehaviour
    {
        [ShowInInspector, Searchable]
        private Dictionary<object, AssetHandler> LoadedHandlers
        {
            get => AddressableLoader._assets;
            set { }
        }

        [ShowInInspector, Searchable]
        private Dictionary<object, SceneInstance> LoadedScenes
        {
            get => AddressableLoader._scenes
                .Where(x => x.Value.IsValid())
                .Select(t => new { t.Key, t.Value.Result })
                .ToDictionary(t => t.Key, t => t.Result);
            set { }
        }

        [ShowInInspector, Searchable]
        private List<string> NativeLoadedAllAddressables
        {
            get
            {
                return AddressableLoader.NativeLoadedAllAddressablesList;
            }
            set { }
        }

        private void Start()
        {
            //AddressableLoader.IsDebug = true;
        }

        [Button]
        private void InstantiateGameObject(AssetReferenceGameObject assetReference, AssetLoadParams assetLoadParams, GameObject relatedGameObject)
        {
            AddressableLoader.InstantiateGameObject(assetReference, assetLoadParams, null, Vector3.zero, Quaternion.identity, relatedGameObject);
        }
        
        [Button]
        private void TryGetOrLoadAsset(AssetReference assetReference, AssetLoadParams assetLoadParams)
        {
            AddressableLoader.TryGetOrLoadAssetWithPredicate<UnityEngine.Object>(assetReference, assetLoadParams, null);
        }

        [Button]
        private void ReleaseAllManaged()
        {
            AddressableLoader.ReleaseAllManaged();
        }

        [Button]
        private void ReleaseAllNativeAddressables()
        {
            AddressableLoader.ReleaseAllNativeAddressables();
        }
    }
}