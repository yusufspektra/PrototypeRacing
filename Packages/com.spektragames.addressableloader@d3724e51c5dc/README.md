# Addressable Loader
Enhanced addressable loader system. With this package you can easily manage and debug your addressable loaded assets.

# Dependencies
- UniTask
- Unity Addressables
- Spektra Utilities

# General Usage
- For any Addressables.XXX methods, you should use `AddressableLoader.XXX` instead.
- For AssetReference or AssetReferenceT<T>, you should use `LoadableAssetReference<T>` field.

# AddressableLoader Main Class
- `AddressableLoader.TryGetOrLoadAssetWithPredicate<>`: Loads get provide you unity object as T. T is can't be a component. This functions returns you LoadResponse<T> object by this object you can check object is loaded success or not. Example usage:
  ```
    [SerializeField] private LoadableAssetReference<GameObject> _myAssetReference;
    [SerializeField] private MeshRenderer _myMeshRederer;
    private async UniTask MyLoadFunction()
    {
        LoadResponse<Texture2D> loadedTexture = await AddressableLoader.TryGetOrLoadAssetWithPredicate<Texture2D>(_myAssetReference, new AssetLoadParams()
        {
            methodType = AssetReleaseMethodType.ByReferenceCount,
            useDelayForReleaseHandle = true,
            delay = 1f
        }, gameObject);

        if (!loadedTexture.IsSuccess)
        {
            // Object not loaded
            return;
        }

        _myMeshRederer.material.mainTexture = loadedTexture.Result;
    }
  ```
- `AddressableLoader.InstantiateGameObject`: Loads and instantiate game object. The usage of this method is same with AddressableLoader.TryGetOrLoadAssetWithPredicate<>.
- `AddressableLoader.InstantiateGameObjectWithPredicate`: Loads and instantiate game object with livenessPredicate(Check livenessPredicate section). The usage of this method is same with AddressableLoader.TryGetOrLoadAssetWithPredicate<>.
- `AddressableLoader.ReleaseInstanceOfAssetReference`: Release single instance of asset reference. If all release methods of this asset handle are 'ByReferenceCount' and reference count decreases to 0, the asset will release from memory. 
- `AddressableLoader.ReleaseAllInstancesOfAssetReference`: Force release all instances for specified asset reference and release from memory completely.
- `AddressableLoader.ReleaseAllManaged`: Release all assets that managed by AddressableLoader script.
- `AddressableLoader.ReleaseAllNativeAddressables`: Release all assets include managed and unmanaged too.
- `AddressableLoader.LoadSceneAsync / AddressableLoader.UnloadSceneAsync`: Load/Unload scenes.

# LoadableAssetReference < T > Field
- Use this field instead of AssetReference and AssetReferenceT< T > fields.
- The all methods in LoadableAssetReference directly using AddressableLoader methods. Available methods for LoadableAssetReference:
  - `LoadableAssetReference.LoadAssetAsync`
  - `LoadableAssetReference.InstantiateAsync`
  - `LoadableAssetReference.InstantiateAsyncWithPredicate`
  - `LoadableAssetReference.ReleaseInstance` (Calls AddressableLoader.ReleaseInstanceOfAssetReference)
  - `LoadableAssetReference.ReleaseAsset` (Calls AddressableLoader.ReleaseAllInstancesOfAssetReference)
  - `LoadableAssetReference.UnloadSceneAsync`
  - `LoadableAssetReference.LoadSceneAsyncTask`

# AssetLoadParams
- This param class is required by all Load and Instantiate methods.
- There are 4 types of asset release method, you can find them in AssetReleaseMethodType enum:
  - `ByReferenceCount`: When you load an asset, a reference count will be increase for this asset. This reference number will decrease as you release the asset. If the reference count decreases to 0, the asset will release from memory completely.
  - `WhenGameObjectDestroyed`: The asset instance will release when specified game object destroyed. DON'T CONFUSE THIS RELEASE METHOD TYPE WITH THE `GameObject relatedGameObject` PARAM OF LOAD/INSTANTIATE METHODS.
  - `WhenGameObjectDisabled`: The asset instance will release when specified game object deactivated/disabled. Also, it will work when game object destroyed.
  - `WhenActiveSceneChanged`: The asset instance will release when active scene changed.
- If you choose `WhenGameObjectDestroyed` or `WhenGameObjectDisabled` release methods, you should fill `AssetLoadParams.objectToTrack` GameObject field. The Loader will track and listen Destroying or Disabling callbacks of this gameObject depending on your choice.
- By activate the `AssetLoadParams.useDelayForReleaseHandle`, you can delay the unloading of the main asset even though all references of the AssetReference are released. If you activate this option; you should set `AssetLoadParams.delay` variable, it's uses seconds. Example use case:
  - You have two screen, both screen loads by Addressable. Both of them contain common addressable assets like button template. When you switch from one screen to another, you release one and load the other. According to this situation, common assets will be released first and then reloaded immediately. To prevent this, you can use `AssetLoadParams.useDelayForReleaseHandle` to prevent the assets from being immediately deleted from memory. And of course you should load/instantiate common assets with different handle.

# relatedGameObject Parameter
- All load and instantiate methods support for this GameObject parameter.
- If you try to lo or instantiate an asset, it can take time sometimes. And finally after the object is loaded/spawned as async it may be too late for some things. For example; you are in multiplayer mode, and you try to load car of a player async. Load process took 5 seconds and when object loaded you see that the player has left the game. So now you need to release the loaded object...
- To make things easier; you can pass spawned main player object in Load/Instantiate methods with `relatedGameObject` parameter. If main player object destroyed in load process, then the asset you try to load will release automatically.
- When `relatedGameObject` is destroyed in the loading process of asset; the `LoadResponse<T>.IsSuccess` will return false, you can use this.
- This is not the same as what `AssetReleaseMethodType.WhenGameObjectDestroyed` does. The `relatedGameObject` parameter is only used when the asset you want to load is in the loading phase. If the relatedGameObject is destroyed after the asset loading is completed successfully, the asset cannot be released automatically.
- You can pass null(as default) this parameter in all load and instantiate methods.

# Func<bool> livenessPredicate Parameter
- It's almost same with relatedGameObject parameter. relatedGameObject param is using Func<bool> livenessPredicate in internal code.
- The first time the object is loaded into memory asynchronously may take some time. Immediately after loading, the value returned by livenessPredicate is checked. If it returns false, the object is released from memory and the relevant load/instantiate method returns negative.
- You can pass null(as default) this parameter in all load and instantiate methods.

# AddressableLoaderDebugger Component
- You can see all managed assets by AddressableLoader with this component.
