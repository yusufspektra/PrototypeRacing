using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace SpektraGames.AddressableLoader.Runtime
{
    [System.Serializable]
    public class ProvidedAsset : IDisposable
    {
        [System.NonSerialized] public AssetHandler assetHandler = null;
        public AssetLoadParams loadParams = null;

        internal UnityEngine.Object instantiatedObject = null; // For game objects

        private GameObjectTracker _gameObjectTracker = null;
        private bool _subscribedForOnDestroyed = false;
        private bool _subscribedForOnDisabled = false;
        private bool _subscribedForOnSceneChange = false;

        public ProvidedAsset(AssetHandler assetHandler, AssetLoadParams loadParams)
        {
            this.assetHandler = assetHandler;
            this.loadParams = loadParams;
        }

        internal void Initialize()
        {
            if (InitialChecks())
            {
                if (loadParams.methodType == AssetReleaseMethodType.WhenGameObjectDestroyed)
                {
                    _gameObjectTracker = loadParams.objectToTrack.GetOrAddComponent<GameObjectTracker>();
                    _gameObjectTracker.onDestroyed += OnTargetObjectDestroyed;
                    _subscribedForOnDestroyed = true;
                }
                else if (loadParams.methodType == AssetReleaseMethodType.WhenGameObjectDisabled)
                {
                    _gameObjectTracker = loadParams.objectToTrack.GetOrAddComponent<GameObjectTracker>();
                    _gameObjectTracker.onDisabled += OnTargetObjectDisabled;
                    _subscribedForOnDisabled = true;
                }
                else if (loadParams.methodType == AssetReleaseMethodType.WhenActiveSceneChanged)
                {
                    SceneManager.activeSceneChanged += OnActiveSceneChanged;
                    _subscribedForOnSceneChange = true;
                }
            }
        }

        private bool InitialChecks()
        {
            switch (loadParams.methodType)
            {
                case AssetReleaseMethodType.ByReferenceCount:
                    break;
                case AssetReleaseMethodType.WhenGameObjectDestroyed:
                case AssetReleaseMethodType.WhenGameObjectDisabled:
                    if (loadParams.objectToTrack == null)
                    {
                        Debug.LogError("loadParams.objectToTrack is null");
                        return false;
                    }

                    break;
                case AssetReleaseMethodType.WhenActiveSceneChanged:
                    break;
                default:
                    Debug.LogError("Undefined AssetReleaseMethodType: " + loadParams.methodType);
                    return false;
            }

            return true;
        }

        private void OnTargetObjectDestroyed()
        {
            try
            {
                if (assetHandler != null)
                    AddressableLoader.Logger.Log("Release Triggered For OnTargetObjectDestroyed: " + AddressableLoader.SerializeAssetRefForDebug(assetHandler.assetReference));
            }
            catch
            {
                // ignored
            }

            if (_gameObjectTracker != null)
                _gameObjectTracker.onDestroyed -= OnTargetObjectDestroyed;
            _subscribedForOnDestroyed = false;
            Release();
        }

        private void OnTargetObjectDisabled()
        {
            try
            {
                if (assetHandler != null)
                    AddressableLoader.Logger.Log("Release Triggered For OnTargetObjectDisabled: " + AddressableLoader.SerializeAssetRefForDebug(assetHandler.assetReference));
            }
            catch
            {
                // ignored
            }
            
            if (_gameObjectTracker != null)
                _gameObjectTracker.onDisabled -= OnTargetObjectDisabled;
            _subscribedForOnDisabled = false;
            Release();
        }

        private void OnActiveSceneChanged(Scene from, Scene to)
        {
            try
            {
                if (assetHandler != null)
                    AddressableLoader.Logger.Log("Release Triggered For OnActiveSceneChanged: " + AddressableLoader.SerializeAssetRefForDebug(assetHandler.assetReference));
            }
            catch
            {
                // ignored
            }
            
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            _subscribedForOnSceneChange = false;
            Release();
        }

        [Button]
        public void Release()
        {
            assetHandler?.ReleaseProvidedAsset(this);
        }

        private void CleanUp()
        {
            // This class object destroying, clean up everything

            if (_subscribedForOnDestroyed && _gameObjectTracker != null)
                _gameObjectTracker.onDestroyed -= OnTargetObjectDestroyed;

            if (_subscribedForOnDisabled && _gameObjectTracker != null)
                _gameObjectTracker.onDisabled -= OnTargetObjectDisabled;

            if (_subscribedForOnSceneChange)
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            _subscribedForOnDestroyed = false;
            _subscribedForOnDisabled = false;
            _subscribedForOnSceneChange = false;

            if (instantiatedObject != null)
                GameObject.Destroy(instantiatedObject);
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

        ~ProvidedAsset()
        {
            Dispose(false);
        }

        #endregion
    }
}