using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SpektraGames.AddressableLoader.Runtime
{
    [System.Serializable]
    public class AssetHandler : IDisposable
    {
        public enum LoadStatusType
        {
            Loading = 0,
            Loaded = 1,
            Unloaded = 2
        }

        public AssetReference assetReference = null;

        private LoadStatusType _loadStatus = LoadStatusType.Unloaded;
        [ShowInInspector] public LoadStatusType LoadStatus => _loadStatus;

        private bool _isSprite = false;
        private AsyncOperationHandle<UnityEngine.Object> _operationHandle; // General
        private AsyncOperationHandle<Sprite> _operationHandleSprite; // Sprite
        private AsyncOperationHandle CurrentOperationHandle
        {
            get
            {
                if (_isSprite)
                {
                    return _operationHandleSprite;
                }
                else
                {
                    return _operationHandle;
                }
            }
        }

        [ShowInInspector] private UnityEngine.Object _loadedObject = null;

        internal int waitersForLoading = 0;

        private bool _includeDelayedReleaseMethod = false;
        private float _delay = 0f;
        private CancellationTokenSource _ctsForDelayedReleaseMethod = null;

        public List<ProvidedAsset> providedAssets = new List<ProvidedAsset>();

        internal void Initialize()
        {
        }

        internal async UniTask WaitForLoading()
        {
            waitersForLoading += 1;
            await UniTask.WaitUntil(() => LoadStatus != LoadStatusType.Loading);
            waitersForLoading -= 1;
        }

        internal async UniTask<bool> LoadAsync()
        {
            if (!assetReference.RuntimeKeyIsValid())
            {
                Debug.LogError("Asset reference is invalid");
                _loadStatus = LoadStatusType.Unloaded;
                return false;
            }

            if (LoadStatus == LoadStatusType.Loaded &&
                CurrentOperationHandle.IsValid() &&
                CurrentOperationHandle.Status == AsyncOperationStatus.Succeeded &&
                _loadedObject != null)
            {
                // Already loaded
                return true;
            }

            // Load
            waitersForLoading += 1;
            _loadStatus = LoadStatusType.Loading;

            if (!string.IsNullOrEmpty(assetReference.SubObjectName))
            {
                string serializedAssetReference = assetReference.SerializeObjectWithUnityJson();
                //Debug.LogError(serializedAssetReference);
                if (serializedAssetReference.Contains("UnityEngine.Sprite"))
                {
                    _isSprite = true;
                }

                // const string key = "m_SubObjectType\":\"";
                // int keyIndex = serializedAssetReference.IndexOf(key, StringComparison.Ordinal);
                //
                // if (keyIndex >= 0)
                // {
                //     int start = keyIndex + key.Length;
                //     int end = serializedAssetReference.IndexOf('"', start); // find closing quote
                //
                //     if (end > start)
                //     {
                //         string result = serializedAssetReference.Substring(start, end - start);
                //         if (result.Contains("UnityEngine.Sprite"))
                //         {
                //             _isSprite = true;
                //         }
                //     }
                // }
            }

            if (_isSprite)
            {
                _operationHandleSprite = Addressables.LoadAssetAsync<Sprite>(assetReference);

                await _operationHandleSprite;

                if (!_operationHandleSprite.IsDone)
                {
                    _loadStatus = LoadStatusType.Unloaded;
                    Debug.LogError("_operationHandleSprite could not be completed");
                    waitersForLoading -= 1;
                    return false;
                }

                _loadedObject = _operationHandleSprite.Result;
            }
            else
            {
                _operationHandle = Addressables.LoadAssetAsync<UnityEngine.Object>(assetReference);

                await _operationHandle;

                if (!_operationHandle.IsDone)
                {
                    _loadStatus = LoadStatusType.Unloaded;
                    Debug.LogError("_operationHandle could not be completed");
                    waitersForLoading -= 1;
                    return false;
                }

                _loadedObject = _operationHandle.Result;
            }

            _loadStatus = LoadStatusType.Loaded;
            waitersForLoading -= 1;

            return true;
        }

        internal T Provide<T>(AssetLoadParams loadParams, out ProvidedAsset providedAsset) where T : UnityEngine.Object
        {
            if (LoadStatus == LoadStatusType.Loaded &&
                CurrentOperationHandle.IsValid() &&
                CurrentOperationHandle.Status == AsyncOperationStatus.Succeeded &&
                _loadedObject != null)
            {
                // Loaded

                if (_loadedObject is T castedObject)
                {
                    var providedAssetInstance = new ProvidedAsset(this, loadParams);
                    providedAssets.Add(providedAssetInstance);
                    providedAssetInstance.Initialize();

                    if (_ctsForDelayedReleaseMethod != null)
                    {
                        _ctsForDelayedReleaseMethod.Cancel();
                        _ctsForDelayedReleaseMethod.Dispose();
                        _ctsForDelayedReleaseMethod = null;
                    }

                    if (loadParams.useDelayForReleaseHandle)
                    {
                        _includeDelayedReleaseMethod = true;
                        _delay = loadParams.delay;
                    }

                    providedAsset = providedAssetInstance;
                    return castedObject;
                }
                else
                {
                    Debug.LogError($"The object you want to use has different type. " +
                                   $"Type of object you want: {typeof(T).Name}, " +
                                   $"loaded object type: {_loadedObject.GetType().Name}, " +
                                   $"loaded object name: {_loadedObject.name}");
                    providedAsset = null;
                    return null;
                }
            }
            else
            {
                providedAsset = null;
                return null;
            }
        }

        internal void ReleaseProvidedAsset(ProvidedAsset providedAsset)
        {
            for (var i = 0; i < providedAssets.Count; i++)
            {
                if (providedAssets[i] == providedAsset)
                {
                    providedAssets.RemoveAt(i);
                    providedAsset.Dispose();
                    break;
                }
            }

            if (providedAssets.Count <= 0)
            {
                if (_includeDelayedReleaseMethod && _ctsForDelayedReleaseMethod == null)
                {
                    // Start delay to release handle
                    _ctsForDelayedReleaseMethod = new CancellationTokenSource();
                    UnloadAfterDelay(_delay).AttachExternalCancellation(_ctsForDelayedReleaseMethod.Token).Forget();
                }
                else
                {
                    AddressableLoader.TryRemoveAssetHandler(assetReference, false);
                }
            }
        }

        [Button]
        public void ReleaseHandle()
        {
            AddressableLoader.TryRemoveAssetHandler(assetReference, false);
        }

        private async UniTask UnloadAfterDelay(float delay)
        {
            await UniTask.WaitForSeconds(delay, true).AttachExternalCancellation(_ctsForDelayedReleaseMethod.Token);

            if (_ctsForDelayedReleaseMethod != null)
            {
                _ctsForDelayedReleaseMethod.Dispose();
                _ctsForDelayedReleaseMethod = null;
            }

            AddressableLoader.TryRemoveAssetHandler(assetReference, false);
        }

        private void ForceUnload()
        {
            if (CurrentOperationHandle.IsValid())
            {
                if (CurrentOperationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(CurrentOperationHandle);
                }
            }
        }

        private void CleanUp()
        {
            // This class object destroying, clean up everything

            for (int i = 0; i < providedAssets.Count; i++)
            {
                providedAssets[i].Dispose();
            }

            ForceUnload();

            assetReference = null;
            _loadedObject = null;

            if (_ctsForDelayedReleaseMethod != null)
            {
                try
                {
                    _ctsForDelayedReleaseMethod.Cancel();
                    _ctsForDelayedReleaseMethod.Dispose();
                    _ctsForDelayedReleaseMethod = null;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }

        #region IDisposable Members and Helpers

        private bool disposed = false;

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

        ~AssetHandler()
        {
            Dispose(false);
        }

        #endregion
    }
}