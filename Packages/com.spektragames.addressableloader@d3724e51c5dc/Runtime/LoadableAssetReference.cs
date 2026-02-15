using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SpektraGames.AddressableLoader.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SpektraGames.AddressableLoader.Runtime
{
    [System.Serializable]
    //[InlineEditor]
    public class LoadableAssetReference<TObject> : AssetReferenceT<TObject> where TObject : UnityEngine.Object
    {
        public LoadableAssetReference(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        
        public new TObject editorAsset
        {
            get
            {
                return base.editorAsset as TObject;
            }
        }

        public override bool SetEditorAsset(Object value)
        {
            if (value)
            {
                if (typeof(TObject) == typeof(Sprite))
                {
                    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(value);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        UnityEngine.Object mainAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPath);
                        if (mainAsset)
                        {
                            if (value != mainAsset)
                            {
                                // Debug.LogError(value.name);
                                // Debug.LogError(assetPath);
                                // Debug.LogError(mainAsset.name);
                                
                                var baseType = typeof(AssetReference);
                                
                                var subObjectNameField = baseType.GetField("m_SubObjectName",
                                    BindingFlags.Instance | BindingFlags.NonPublic);

                                var subObjectTypeField = baseType.GetField("m_SubObjectType",
                                    BindingFlags.Instance | BindingFlags.NonPublic);
                                
                                var editorAssetChanged = baseType.GetField("m_EditorAssetChanged",
                                    BindingFlags.Instance | BindingFlags.NonPublic);

                                m_AssetGUID = UnityEditor.AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
                                CachedAsset = value;
                                subObjectNameField?.SetValue(this, value.name);
                                subObjectTypeField?.SetValue(this, typeof(Sprite).AssemblyQualifiedName);
                                editorAssetChanged?.SetValue(this, true);
                                    
                                return true;
                            }
                        }
                    }
                }
            }
            return base.SetEditorAsset(value);
        }
#endif

        [Obsolete]
        public override AsyncOperationHandle<TObject> LoadAssetAsync()
        {
            Debug.LogError("Use LoadAssetAsync(AssetLoadParams, GameObject) method instead of this!");
            return base.LoadAssetAsync();
        }

        [Obsolete]
        public override AsyncOperationHandle<TObject> LoadAssetAsync<TObject>()
        {
            Debug.LogError("Use LoadAssetAsync(AssetLoadParams, GameObject) method instead of this!");
            return base.LoadAssetAsync<TObject>();
        }

        [Obsolete]
        public override AsyncOperationHandle<GameObject> InstantiateAsync(Transform parent = null,
            bool instantiateInWorldSpace = false)
        {
            Debug.LogError(
                "Use InstantiateAsync(AssetLoadParams, Transform, Vector3, Quaterinon, GameObject) method instead of this!");
            return base.InstantiateAsync(parent, instantiateInWorldSpace);
        }

        [Obsolete]
        public override AsyncOperationHandle<SceneInstance> LoadSceneAsync(
            LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            Debug.LogError("Use LoadSceneAsyncTask(LoadSceneMode, Bool, Int) method instead of this!");
            return base.LoadSceneAsync(loadMode, activateOnLoad, priority);
        }

        [Obsolete]
        public override AsyncOperationHandle<SceneInstance> UnLoadScene()
        {
            Debug.LogError("Use UnoadSceneAsync(LoadSceneMode, Bool, Int) method instead of this!");
            return base.UnLoadScene();
        }

        /// <summary>
        /// Load and provide asset
        /// </summary>
        /// <param name="loadParams"></param>
        /// <param name="livenessPredicate">
        /// A function that determines whether the instantiated Asset should remain loaded. The callback will only be tracked until the asset is loaded.
        /// Returns <c>true</c> if the object you want to load should remain in memory; 
        /// returns <c>false</c> if it should be released (e.g., the object is no longer needed or has been destroyed).
        /// </param>
        /// <returns></returns>
        public async UniTask<LoadResponse<TObject>> LoadAssetAsync(
            AssetLoadParams loadParams,
            Func<bool> livenessPredicate = null)
        {
            return await AddressableLoader.TryGetOrLoadAssetWithPredicate<TObject>(this, loadParams, livenessPredicate);
        }

        /// <summary>
        /// Load and Instantiate game object
        /// </summary>
        /// <param name="loadParams"></param>
        /// <param name="parent"></param>
        /// <param name="localPosition"></param>
        /// <param name="localRotation"></param>
        /// <param name="relatedGameObject">If this gameObject is destroyed after asset loaded, the asset will release automatically. The GameObject will only be tracked until the asset is loaded</param>
        /// <returns></returns>
        public async UniTask<LoadResponse<GameObject>> InstantiateAsync(
            AssetLoadParams loadParams,
            Transform parent,
            Vector3 localPosition,
            Quaternion localRotation,
            GameObject relatedGameObject = null)
        {
            return await AddressableLoader.InstantiateGameObject(this, loadParams, parent, localPosition, localRotation,
                relatedGameObject);
        }

        /// <summary>
        /// Load and Instantiate game object
        /// </summary>
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
        public async UniTask<LoadResponse<GameObject>> InstantiateAsyncWithPredicate(
            AssetLoadParams loadParams,
            Transform parent,
            Vector3 localPosition,
            Quaternion localRotation,
            Func<bool> livenessPredicate = null)
        {
            return await AddressableLoader.InstantiateGameObjectWithPredicate(this, loadParams, parent, localPosition,
                localRotation, livenessPredicate);
        }

        /// <summary>
        /// Load scene
        /// </summary>
        /// <param name="loadMode"></param>
        /// <param name="activateOnLoad"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public async UniTask<SceneInstance> LoadSceneAsyncTask(LoadSceneMode loadMode,
            bool activateOnLoad = true,
            IProgress<float> progress = null,
            int priority = 100)
        {
            return await AddressableLoader.LoadSceneAsync(this, loadMode, activateOnLoad, progress, priority);
        }

        /// <summary>
        /// Unload scene
        /// </summary>
        public async UniTask UnloadSceneAsync()
        {
            await AddressableLoader.UnloadSceneAsync(this);
        }

        /// <summary>
        /// Release single instance of asset reference
        /// </summary>
        /// <param name="obj"></param>
        public override void ReleaseInstance(GameObject obj)
        {
            this.ReleaseInstanceOfAssetReference();
        }

        /// <summary>
        /// Release single instance of asset reference
        /// </summary>
        /// <param name="obj"></param>
        public void ReleaseInstance()
        {
            this.ReleaseInstanceOfAssetReference();
        }

        /// <summary>
        /// Release all instances of assetReference provided and unload it
        /// </summary>
        public override void ReleaseAsset()
        {
            this.ReleaseAllInstancesOfAssetReference();
        }
    }
}