using SpektraGames.ObjectPooling.Runtime;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SpektraGames.RuntimeUI.Runtime
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public partial class RuntimeUI
    {
        private static bool _inited = false;
        public static bool Inited => _inited;

        private static bool _quitting = false;
        private static bool Quitting => _quitting;

        private static Canvas _canvas = null;
        public static Canvas Canvas => _canvas;

        private const string ToastsPoolName = "RuntimeUI_Toasts";
        internal static readonly int _toastsPoolId = ToastsPoolName.GetHashCode();

        private const string PopupsPoolName = "RuntimeUI_Popups";
        internal static readonly int _popupsPoolId = PopupsPoolName.GetHashCode();

#if UNITY_EDITOR
        static RuntimeUI()
        {
            // Subscribe to the playModeStateChanged event
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }
#endif

        public static void Init()
        {
            if (!CanCallRuntimeMethod)
            {
                return;
            }

            if (_inited)
            {
                //Debug.LogError("Already inited");
                return;
            }

            if (!RuntimeUISettings.Instance)
            {
                Debug.LogError("RuntimeUISettings not found in project");
                return;
            }

            var canvasObject = Resources.Load<GameObject>("RuntimeUI/Canvas_RuntimeUI");
            var canvasInstantiated = GameObject.Instantiate(canvasObject).GetComponent<Canvas>();
            var poolParent = canvasInstantiated.transform.Find("_Pool");
            GameObject.DontDestroyOnLoad(canvasInstantiated.gameObject);

            canvasInstantiated.sortingOrder = RuntimeUISettings.Instance.CanvasSortOrder;
            if (canvasInstantiated.TryGetComponent<CanvasScaler>(out var canvasScaler))
            {
                var canvasReferenceResolution = RuntimeUISettings.Instance.CanvasReferenceResolution;
                canvasScaler.referenceResolution =
                    new Vector2(canvasReferenceResolution.x, canvasReferenceResolution.y);
            }

            _canvas = canvasInstantiated;

            var loadingToSpawn = RuntimeUISettings.Instance.FullScreenLoadingBehaviourReference;
            CurrentFullScreenLoadingBehaviour = GameObject.Instantiate(loadingToSpawn, _canvas.transform);
            var loadingRectTransform = CurrentFullScreenLoadingBehaviour.transform as RectTransform;
            loadingRectTransform.localScale = Vector3.one;
            loadingRectTransform.SetLRTB(Vector4.zero);
            
            var blockScreenToSpawn = Resources.Load<BlockScreenBehaviour>("RuntimeUI/Base_BlockScreen");
            CurrentBlockScreenBehaviour = GameObject.Instantiate(blockScreenToSpawn, _canvas.transform);
            var blockRectTransform = CurrentBlockScreenBehaviour.transform as RectTransform;
            blockRectTransform.localScale = Vector3.one;
            blockRectTransform.SetLRTB(Vector4.zero);

            var toastsToSpawn = RuntimeUISettings.Instance.NumberOfToastsToSpawnAtStart;
            var poolPropertiesForToasts = new PoolObjectProperties()
            {
                initialSize = toastsToSpawn,
                maxPoolSize = RuntimeUISettings.Instance.MaxConcurrentToasts,
                poolIncreaseSize = 1,
                poolOptions = PoolOptions.TriggerPoolCallbacks
            };
            IndependentPoolManager.RegisterObjectPool(
                ToastsPoolName,
                _toastsPoolId,
                RuntimeUISettings.Instance.ToastBehaviourReference.gameObject,
                poolPropertiesForToasts,
                poolParent);

            var popupsToSpawn = RuntimeUISettings.Instance.NumberOfPopupsToSpawnAtStart;
            var poolPropertiesForPopups = new PoolObjectProperties()
            {
                initialSize = popupsToSpawn,
                maxPoolSize = 5,
                poolIncreaseSize = 1,
                poolOptions = PoolOptions.TriggerPoolCallbacks
            };
            IndependentPoolManager.RegisterObjectPool(
                PopupsPoolName,
                _popupsPoolId,
                RuntimeUISettings.Instance.DynamicPopupBehaviourReference.gameObject,
                poolPropertiesForPopups,
                poolParent);

            _inited = true;
        }

        private static bool CanCallRuntimeMethod
        {
            get
            {
                if (!Application.isPlaying)
                {
                    return false;
                }

                if (Quitting)
                {
                    return false;
                }

#if UNITY_EDITOR
                if (!EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying)
                {
                    return false;
                }
#endif

                return true;
            }
        }

#if UNITY_EDITOR
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _quitting = true;
            }
            else if (state == PlayModeStateChange.ExitingEditMode ||
                     state == PlayModeStateChange.EnteredPlayMode)
            {
                _quitting = false;
            }
        }
#endif
    }
}