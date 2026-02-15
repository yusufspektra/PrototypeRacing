using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace SpektraGames.AddressableLoader.Runtime
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class AddressableLoader
    {
        internal static Dictionary<object, AssetHandler> _assets = new();
        internal static Dictionary<object, AsyncOperationHandle<SceneInstance>> _scenes = new();
        internal static InfoLogger Logger { get; set; } = new InfoLogger("AddressableLoader", "magenta");

        private static bool _triedToLoadMapFileBefore = false;
        private static List<AssetEntryForDebug> _assetsEntriesMap = null;
        private static List<AssetEntryForDebug> AssetsEntriesMap
        {
            get
            {
                if (!_triedToLoadMapFileBefore)
                {
                    _triedToLoadMapFileBefore = true;
                    string assetMappingFilePath = "AddressableLoader/AddressableMapping";
                    TextAsset textAsset = Resources.Load<TextAsset>(assetMappingFilePath);
                    if (textAsset != null)
                    {
                        _assetsEntriesMap = JsonConvert.DeserializeObject<List<AssetEntryForDebug>>(textAsset.text);
                    }
                }

                return _assetsEntriesMap;
            }
        }

        static AddressableLoader()
        {
#if UNITY_EDITOR
            _assets = new();
            _scenes = new();
#endif
        }

        private static bool CheckGameObjectAlive(GameObjectTracker tracker)
        {
            return tracker != null && !tracker.Destroyed;
        }

        /// <summary>
        /// Load and Instantiate game object
        /// </summary>
        /// <param name="assetReference"></param>
        /// <param name="loadParams"></param>
        /// <param name="parent"></param>
        /// <param name="localPosition"></param>
        /// <param name="localRotation"></param>
        /// <param name="relatedGameObject">If this gameObject is destroyed after asset loaded, the asset will release automatically. The GameObject will only be tracked until the asset is loaded</param>
        /// <returns></returns>
        public static async UniTask<LoadResponse<GameObject>> InstantiateGameObject(
            AssetReference assetReference,
            AssetLoadParams loadParams,
            Transform parent,
            Vector3 localPosition,
            Quaternion localRotation,
            GameObject relatedGameObject = null)
        {
            if (relatedGameObject != null)
            {
                GameObjectTracker tracker = relatedGameObject.GetOrAddComponent<GameObjectTracker>();
                using (var validationHelper = new GameObjectValidationHelper(tracker))
                {
                    return await InstantiateGameObjectWithPredicate(assetReference, loadParams, parent, localPosition,
                        localRotation, validationHelper.IsObjectAlive);
                }
            }
            else
            {
                return await InstantiateGameObjectWithPredicate(assetReference, loadParams, parent, localPosition,
                    localRotation, livenessPredicate: null);
            }
        }

        /// <summary>
        /// Load and Instantiate game object
        /// </summary>
        /// <param name="assetReference"></param>
        /// <param name="loadParams"></param>
        /// <param name="parent"></param>
        /// <param name="localPosition"></param>
        /// <param name="localRotation"></param>
        /// <param name="livenessPredicate">
        /// A function that determines whether the instantiated GameObject should remain loaded. The callback will only be tracked until the asset is loaded.
        /// Returns <c>true</c> if the object you want to load should remain in memory; 
        /// returns <c>false</c> if it should be released (e.g., the object is no longer needed or has been destroyed).
        /// </param>
        /// <returns></returns>
        public static async UniTask<LoadResponse<GameObject>> InstantiateGameObjectWithPredicate(
            AssetReference assetReference,
            AssetLoadParams loadParams,
            Transform parent,
            Vector3 localPosition,
            Quaternion localRotation,
            Func<bool> livenessPredicate = null)
        {
            var loadResponse =
                await TryGetOrLoadAssetWithPredicate<GameObject>(assetReference, loadParams, livenessPredicate);
            if (!loadResponse.IsSuccess)
                return loadResponse;

            // Object loaded
            var spawnedObject = GameObject.Instantiate(loadResponse.Result);
            if (parent != null)
                spawnedObject.transform.SetParent(parent);
            spawnedObject.transform.localPosition = localPosition;
            spawnedObject.transform.localRotation = localRotation;

            loadResponse.ProvidedAssetHandle.instantiatedObject = spawnedObject;

            return new LoadResponse<GameObject>(isSuccess: true, result: spawnedObject,
                providedAssetHandle: loadResponse.ProvidedAssetHandle);
        }

        /// <summary>
        /// Load and provide asset
        /// </summary>
        /// <param name="assetReference"></param>
        /// <param name="loadParams"></param>
        /// <param name="livenessPredicate">
        /// A function that determines whether the instantiated Asset should remain loaded. The callback will only be tracked until the asset is loaded.
        /// Returns <c>true</c> if the object you want to load should remain in memory; 
        /// returns <c>false</c> if it should be released (e.g., the object is no longer needed or has been destroyed).
        /// </param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async UniTask<LoadResponse<T>> TryGetOrLoadAssetWithPredicate<T>(
            AssetReference assetReference,
            AssetLoadParams loadParams,
            Func<bool> livenessPredicate = null)
            where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Logger.LogError("You can't use in editor mode");
                return new LoadResponse<T>(false, null, null);
            }
#endif

           // Debug.LogError(assetReference.SerializeObjectWithUnityJson());

            DateTime startTime = DateTime.Now;

            if (loadParams == null)
            {
                Logger.LogError("loadParams is null");
                return new LoadResponse<T>(false, null, null);
            }

            if (assetReference == null)
            {
                Logger.LogError("assetReference is null");
                return new LoadResponse<T>(false, null, null);
            }

            if (!assetReference.RuntimeKeyIsValid())
            {
                Logger.LogError("assetReference runtime key is not valid");
                return new LoadResponse<T>(false, null, null);
            }

            Logger.Log(
                $"Start load asset: {SerializeAssetRefForDebug(assetReference)})");

            if (_assets.TryGetValue(assetReference.RuntimeKey, out AssetHandler currentAssetHandler))
            {
                // The asset already loading/loaded

                if (currentAssetHandler.LoadStatus == AssetHandler.LoadStatusType.Loading)
                {
                    Logger.Log(
                        $"Asset already loading. Wait for load: {SerializeAssetRefForDebug(assetReference)})");

                    // The asset loading, wait for it
                    await currentAssetHandler.WaitForLoading();
                    bool isLoaded = currentAssetHandler.LoadStatus == AssetHandler.LoadStatusType.Loaded;
                    if (!isLoaded)
                    {
                        Logger.Log(
                            $"loading asset(method: wait for existing handle) failed in {(DateTime.Now - startTime).TotalMilliseconds} ms: {SerializeAssetRefForDebug(assetReference)})");

                        // Asset not laoded because of an error occured
                        return new LoadResponse<T>(isSuccess: false, result: null, providedAssetHandle: null);
                    }

                    if (livenessPredicate != null && livenessPredicate() == false)
                    {
                        Logger.Log(
                            $"asset loaded(method: wait for existing handle) but predicate function returned false. Load time {(DateTime.Now - startTime).TotalMilliseconds} ms: {SerializeAssetRefForDebug(assetReference)})");

                        // Predicate function returned false, this asset should relese
                        TryRemoveAssetHandler(assetReference, true);
                        return new LoadResponse<T>(isSuccess: false, result: null, providedAssetHandle: null);
                    }

                    Logger.Log(
                        $"asset loaded(method: wait for existing handle) in {(DateTime.Now - startTime).TotalMilliseconds} ms: {SerializeAssetRefForDebug(assetReference)})");

                    // Provide the asset
                    return new LoadResponse<T>(isSuccess: true,
                        result: currentAssetHandler.Provide<T>(loadParams, out ProvidedAsset providedAsset),
                        providedAssetHandle: providedAsset);
                }
                else if (currentAssetHandler.LoadStatus == AssetHandler.LoadStatusType.Unloaded)
                {
                    Logger.Log(
                        $"asset not loaded. Passed time {(DateTime.Now - startTime).TotalMilliseconds} ms: {SerializeAssetRefForDebug(assetReference)})");

                    // Seems like we have an error
                    return new LoadResponse<T>(isSuccess: false, result: null, providedAssetHandle: null);
                }
                else if (currentAssetHandler.LoadStatus == AssetHandler.LoadStatusType.Loaded)
                {
                    // The asset loaded

                    Logger.Log(
                        $"asset loaded(method: already loaded) in {(DateTime.Now - startTime).TotalMilliseconds} ms: {SerializeAssetRefForDebug(assetReference)})");

                    // Provide the asset
                    return new LoadResponse<T>(isSuccess: true,
                        result: currentAssetHandler.Provide<T>(loadParams, out ProvidedAsset providedAsset),
                        providedAssetHandle: providedAsset);
                }
            }
            else
            {
                Logger.Log(
                    $"Asset not exist in memory. Wait for load: {SerializeAssetRefForDebug(assetReference)})");

                // Asset not loaded before
                AssetHandler newAssetHandler = new AssetHandler();
                newAssetHandler.assetReference = assetReference;
                _assets[assetReference.RuntimeKey] = newAssetHandler;
                newAssetHandler.Initialize();
                newAssetHandler.waitersForLoading++;
                bool isLoaded = await newAssetHandler.LoadAsync();
                newAssetHandler.waitersForLoading--;
                if (!isLoaded)
                {
                    // Asset not loaded because of an error occured
                    if (_assets.TryGetValue(assetReference.RuntimeKey, out AssetHandler assetHandler))
                    {
                        assetHandler.Dispose();
                        _assets.Remove(assetReference.RuntimeKey);
                    }

                    Logger.Log(
                        $"asset not loaded(method: load from scratch). Passed time {(DateTime.Now - startTime).TotalMilliseconds} ms: {SerializeAssetRefForDebug(assetReference)})");

                    return new LoadResponse<T>(isSuccess: false, result: null, providedAssetHandle: null);
                }

                if (livenessPredicate != null && livenessPredicate() == false)
                {
                    Logger.Log(
                        $"asset loaded(method: load from scratch) but predicate function returned false. Load time {(DateTime.Now - startTime).TotalMilliseconds} ms: {SerializeAssetRefForDebug(assetReference)})");

                    // Predicate function returned false, this asset should relese
                    TryRemoveAssetHandler(assetReference, true);
                    return new LoadResponse<T>(isSuccess: false, result: null, providedAssetHandle: null);
                }

                Logger.Log(
                    $"asset loaded(method: load from scratch) in {(DateTime.Now - startTime).TotalMilliseconds} ms: {SerializeAssetRefForDebug(assetReference)})");

                // Provide the asset
                return new LoadResponse<T>(isSuccess: true,
                    result: newAssetHandler.Provide<T>(loadParams, out ProvidedAsset providedAsset),
                    providedAssetHandle: providedAsset);
            }

            return new LoadResponse<T>(isSuccess: false, result: null, providedAssetHandle: null);
        }

        /// <summary>
        /// Load scene
        /// </summary>
        /// <param name="assetReference"></param>
        /// <param name="loadMode"></param>
        /// <param name="activateOnLoad"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static async UniTask<SceneInstance> LoadSceneAsync(
            AssetReference assetReference,
            LoadSceneMode loadMode,
            bool activateOnLoad = true,
            IProgress<float> progress = null,
            int priority = 100)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Logger.LogError("You can't use in editor mode");
                return default;
            }
#endif

            if (assetReference == null)
            {
                Logger.LogError("assetReference is null");
                return default;
            }

            if (!assetReference.RuntimeKeyIsValid())
            {
                Logger.LogError("assetReference runtime key is not valid");
                return default;
            }

            if (_scenes.ContainsKey(assetReference.RuntimeKey))
            {
                Logger.LogError("Scene already loaded: " + assetReference.AssetGUID);
                return default;
            }

            DateTime startTime = DateTime.Now;

            Logger.Log(
                $"LoadSceneAsync started: {SerializeAssetRefForDebug(assetReference)})");

            progress?.Report(0f);
            var handle = Addressables.LoadSceneAsync(assetReference, loadMode, activateOnLoad, priority);
            var toUniTask = handle.ToUniTask(progress);
            var loadResponse = await toUniTask;
            progress?.Report(1f);

            if (handle.IsValid() && handle.IsDone)
            {
                Logger.Log(
                    $"LoadSceneAsync finished in {(DateTime.Now - startTime).TotalMilliseconds} ms: {SerializeAssetRefForDebug(assetReference)})");

                _scenes[assetReference.RuntimeKey] = handle;
                return handle.Result;
            }
            else
            {
                Logger.Log(
                    $"Internal error: {SerializeAssetRefForDebug(assetReference)})");
                return default;
            }
        }

        /// <summary>
        /// Unload scene
        /// </summary>
        /// <param name="assetReference"></param>
        /// <param name="unloadSceneOptions"></param>
        public static async UniTask UnloadSceneAsync(
            AssetReference assetReference,
            UnloadSceneOptions unloadSceneOptions = UnloadSceneOptions.None)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Logger.LogError("You can't use in editor mode");
                return;
            }
#endif

            if (assetReference == null)
            {
                Logger.LogError("assetReference is null");
                return;
            }

            if (!assetReference.RuntimeKeyIsValid())
            {
                Logger.LogError("assetReference runtime key is not valid");
                return;
            }

            DateTime startTime = DateTime.Now;

            Logger.Log(
                $"UnloadSceneAsync started: {SerializeAssetRefForDebug(assetReference)})");

            if (_scenes.TryGetValue(assetReference.RuntimeKey, out AsyncOperationHandle<SceneInstance> handle))
            {
                await Addressables.UnloadSceneAsync(handle, unloadSceneOptions);
                _scenes.Remove(assetReference.RuntimeKey);

                Logger.Log(
                    $"UnloadSceneAsync finished in {(DateTime.Now - startTime).TotalMilliseconds} ms: {SerializeAssetRefForDebug(assetReference)})");
            }
            else
            {
                Logger.LogError("Scene not founded: " + assetReference.AssetGUID);
                return;
            }
        }

        /// <summary>
        /// Check for scene loaded
        /// </summary>
        /// <param name="assetReference"></param>
        /// <param name="sceneInstance"></param>
        /// <returns></returns>
        public static bool IsSceneLoaded(this AssetReference assetReference, out SceneInstance sceneInstance)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Logger.LogError("You can't use in editor mode");
                sceneInstance = default;
                return false;
            }
#endif

            if (assetReference == null)
            {
                Logger.LogError("assetReference is null");
                sceneInstance = default;
                return false;
            }

            if (!assetReference.RuntimeKeyIsValid())
            {
                Logger.LogError("assetReference runtime key is not valid");
                sceneInstance = default;
                return false;
            }

            if (_scenes.TryGetValue(assetReference.RuntimeKey, out AsyncOperationHandle<SceneInstance> handle))
            {
                sceneInstance = handle.Result;
                return true;
            }

            sceneInstance = default;
            return false;
        }

        /// <summary>
        /// Release single instance of asset reference
        /// </summary>
        /// <param name="assetReference"></param>
        public static void ReleaseInstanceOfAssetReference(this AssetReference assetReference)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (assetReference == null)
            {
                Logger.LogError("assetReference is null");
                return;
            }

            if (!assetReference.RuntimeKeyIsValid())
            {
                Logger.LogError("assetReference runtime key is not valid");
                return;
            }

            if (_assets.TryGetValue(assetReference.RuntimeKey, out AssetHandler assetHandler))
            {
                Logger.Log(
                    $"ReleaseInstanceOfAssetReference: {SerializeAssetRefForDebug(assetReference)})");

                for (var i = assetHandler.providedAssets.Count - 1; i >= 0; i--)
                {
                    var releaseMethod = assetHandler.providedAssets[i].loadParams.methodType;
                    if (releaseMethod == AssetReleaseMethodType.ByReferenceCount)
                    {
                        assetHandler.providedAssets[i].Release();
                        break;
                    }
                }
            }
            else
            {
                Logger.LogError("Asset reference not found in current handlers");
            }
        }

        /// <summary>
        /// Release all instances of assetReference provided and unload it
        /// </summary>
        /// <param name="assetReference"></param>
        public static void ReleaseAllInstancesOfAssetReference(this AssetReference assetReference)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Logger.LogError("You can't use in editor mode");
                return;
            }
#endif

            if (assetReference == null)
            {
                Logger.LogError("assetReference is null");
                return;
            }

            if (!assetReference.RuntimeKeyIsValid())
            {
                Logger.LogError("assetReference runtime key is not valid");
                return;
            }

            if (_assets.TryGetValue(assetReference.RuntimeKey, out AssetHandler assetHandler))
            {
                Logger.Log(
                    $"ReleaseAllInstancesOfAssetReference: {SerializeAssetRefForDebug(assetReference)})");

                TryRemoveAssetHandler(assetReference, false);
            }
        }

        /// <summary>
        /// Release all assets that managed by this script
        /// </summary>
        public static void ReleaseAllManaged()
        {
            Logger.Log("ReleaseAllManaged STARTED");

            foreach (var keyValuePair in _assets)
            {
                var key = keyValuePair.Key;
                Logger.Log("Release: " + SerializeAssetRefForDebug(_assets[key].assetReference));
                keyValuePair.Value?.Dispose();
            }

            _assets = new();

            Logger.Log(
                $"ReleaseAllManaged ENDED");
        }

        /// <summary>
        /// Release all assets that managed by this script by label.
        /// So only asset references with the label parameter will be released.
        /// </summary>
        public static async UniTask ReleaseAllManagedByLabel(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                Logger.Log("ReleaseAllManagedByLabel:: label is null or empty");
                return;
            }

            DateTime startTime = DateTime.Now;

            Logger.Log(
                $"ReleaseAllManagedByLabel started: {label})");

            var resourceLocations = await Addressables.LoadResourceLocationsAsync(label);
            if (resourceLocations == null || resourceLocations.Count <= 0)
            {
                Logger.Log("ReleaseAllManagedByLabel:: resourceLocations is null or empty for label '" + label + "'");
                return;
            }

            Logger.Log(
                $"ReleaseAllManagedByLabel resourceLocations fetched in {(DateTime.Now - startTime).TotalMilliseconds} ms: {label})");

            var keys = GetKeysFromLocations(resourceLocations);
            if (keys.IsNullOrEmpty())
            {
                Logger.Log(
                    "ReleaseAllManagedByLabel:: resourceLocations.keys is null or empty for label '" + label + "'");
                return;
            }

            for (var i = 0; i < keys.Count; i++)
            {
                if (_assets.ContainsKey(keys[i]))
                {
                    _assets[keys[i]]?.Dispose();
                    _assets.Remove(keys[i]);
                }
            }

            List<object> GetKeysFromLocations(IList<IResourceLocation> locations)
            {
                List<object> keys = new List<object>(locations.Count);

                foreach (var locator in Addressables.ResourceLocators)
                {
                    foreach (var key in locator.Keys)
                    {
                        bool isGUID = Guid.TryParse(key.ToString(), out var guid);
                        if (!isGUID)
                            continue;

                        if (!TryGetKeyLocationID(locator, key, out var keyLocationID))
                            continue;

                        var locationMatched =
                            locations.Select(x => x.InternalId).ToList().Exists(x => x == keyLocationID);
                        if (!locationMatched)
                            continue;
                        keys.Add(key);
                    }
                }

                return keys;

                bool TryGetKeyLocationID(IResourceLocator locator, object key, out string internalID)
                {
                    internalID = string.Empty;
                    var hasLocation = locator.Locate(key, typeof(UnityEngine.Object), out var keyLocations);
                    if (!hasLocation)
                        return false;
                    if (keyLocations.Count == 0)
                        return false;
                    if (keyLocations.Count > 1)
                        return false;

                    internalID = keyLocations[0].InternalId;
                    return true;
                }
            }
        }

        /// <summary>
        /// Release entire addressables include including those not managed by this script
        /// </summary>
        public static void ReleaseAllNativeAddressables()
        {
            var all = GetAllNativeAsyncOperationHandles();

            for (var i = 0; i < all.Count; i++)
            {
                if (all[i].IsValid())
                {
                    try
                    {
                        Addressables.Release(all[i]);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            ReleaseAllManaged();

            _scenes = new();

            Logger.Log(
                $"ReleaseAllNativeAddressables");
        }

        /// <summary>
        /// Get list of all addressable handles include managed and unmanaged
        /// </summary>
        /// <returns></returns>
        public static List<AsyncOperationHandle> GetAllNativeAsyncOperationHandles()
        {
            var handles = new List<AsyncOperationHandle>();

            var resourceManagerType = Addressables.ResourceManager.GetType();
            var dictionaryMember =
                resourceManagerType.GetField("m_AssetOperationCache", BindingFlags.NonPublic | BindingFlags.Instance);
            var dictionary = dictionaryMember.GetValue(Addressables.ResourceManager) as IDictionary;

            foreach (var asyncOperationInterface in dictionary.Values)
            {
                if (asyncOperationInterface == null)
                    continue;

                var handle = typeof(AsyncOperationHandle).InvokeMember(nameof(AsyncOperationHandle),
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance,
                    null, null, new object[] { asyncOperationInterface });

                handles.Add((AsyncOperationHandle)handle);
            }

            // for (int i = handles.Count - 1; i >= 0; i--)
            // {
            //     if (!handles[i].IsDone || !handles[i].IsValid())
            //     {
            //         handles.RemoveAt(i);
            //     }
            // }

            return handles;
        }

        internal static void TryRemoveAssetHandler(AssetReference assetReference,
            bool checkForLoadingWaitersAndReferences)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (assetReference == null)
                return;

            if (!assetReference.RuntimeKeyIsValid())
                return;

            if (_assets.TryGetValue(assetReference.RuntimeKey, out AssetHandler currentAssetHandler))
            {
                if ((checkForLoadingWaitersAndReferences && currentAssetHandler.waitersForLoading <= 0 &&
                     currentAssetHandler.providedAssets.Count <= 0) ||
                    !checkForLoadingWaitersAndReferences)
                {
                    currentAssetHandler.Dispose();
                    _assets.Remove(assetReference.RuntimeKey);
                }
            }
        }

        internal static string SerializeAssetRefForDebug(AssetReference assetReference)
        {
            if (!Logger.EnableLog)
            {
                return string.Empty;
            }

            if (assetReference == null)
            {
                return "ref null";
            }

            if (!assetReference.RuntimeKeyIsValid())
            {
                return "ref ket not valid";
            }

            string guid = assetReference.AssetGUID.ToLower().Trim();

            if (AssetsEntriesMap != null && AssetsEntriesMap.Any(x => x.guid == guid))
            {
                var entry = AssetsEntriesMap.FirstOrDefault(x => x.guid == guid);

                return $"{entry.assetName}({entry.assetPath})";
            }
            else
            {
                return $"{assetReference.AssetGUID}({assetReference.RuntimeKey.ToString()}";
            }
        }

        public static List<string> ManagedLoadedAllAddressablesList
        {
            get
            {
                List<string> response = new List<string>();

                var handles = _assets;

                foreach (var keyValuePair in handles)
                {
                    var key = keyValuePair.Key;
                    response.Add(
                        $"{handles[key].LoadStatus} => {SerializeAssetRefForDebug(handles[key].assetReference)}");
                }

                return response;
            }
        }

        public static List<string> NativeLoadedAllAddressablesList
        {
            get
            {
                List<string> response = new List<string>();

                var handles = AddressableLoader.GetAllNativeAsyncOperationHandles();
                for (var i = 0; i < handles.Count; i++)
                {
                    var handle = handles[i];
                    bool loaded = handle.IsDone;
                    bool isValid = loaded && handle.IsValid();
                    string addressableName = handle.DebugName;

                    try
                    {
                        if (loaded && isValid && handle.Result != null &&
                            handle.Result is UnityEngine.Object unityObject)
                        {
                            addressableName += " => (" + unityObject.name + "/" + unityObject.GetType().Name + ")";
                        }
                        else if (loaded && isValid && handle.Result != null)
                        {
                            addressableName += " => (" + handle.Result.GetType().Name + ")";
                        }
                    }
                    catch (Exception e)
                    {
                        continue;
                    }

                    string line = null;
                    if (loaded)
                    {
                        if (isValid)
                        {
                            line = "LOADED - " + "VALID - " + addressableName;
                        }
                        else
                        {
                            line = "LOADED - " + "NOT_VALID - " + addressableName;
                        }
                    }
                    else
                    {
                        line = "LOADING - " + "NOT_VALID - " + addressableName;
                    }

                    response.Add(line);
                }

                return response;
            }
            set { }
        }

        private class GameObjectValidationHelper : IDisposable
        {
            private WeakReference<GameObjectTracker> weakReference;
            private bool disposed = false;

            public GameObjectValidationHelper(GameObjectTracker objectToTrack)
            {
                weakReference = new WeakReference<GameObjectTracker>(objectToTrack);
            }

            public bool IsObjectAlive()
            {
                return
                    weakReference != null &&
                    weakReference.TryGetTarget(out var target) &&
                    target != null &&
                    !target.Destroyed;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!this.disposed)
                {
                    if (disposing)
                    {
                        CleanUp();
                    }

                    disposed = true;
                }
            }

            private void CleanUp()
            {
                if (weakReference != null)
                {
                    if (weakReference.TryGetTarget(out var target))
                        weakReference.SetTarget(null);
                    weakReference = null;
                }
            }

            ~GameObjectValidationHelper()
            {
                Dispose(false);
            }
        }
    }
}