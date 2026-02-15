using Sirenix.OdinInspector;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace SpektraGames.RuntimeUI.Runtime
{
    public class RuntimeUISettings : SingletonScriptableObject<RuntimeUISettings>
    {
        [Title("Canvas")] /*//////////////////////////////////////////*/
        [SerializeField]
        private int _canvasSortOrder = 1000;
        public int CanvasSortOrder => _canvasSortOrder;

        [SerializeField]
        private Vector2Int _canvasReferenceResolution = new Vector2Int(1920, 1080);
        public Vector2Int CanvasReferenceResolution => _canvasReferenceResolution;

        [Title("Full Screen Loading")] /*//////////////////////////////////////////*/
        [SerializeField]
        private bool _useDotAnimationForLoading = true;
        public bool UseDotAnimationForLoading => _useDotAnimationForLoading;

        [SerializeField]
        private FullScreenLoadingBehaviour _fullScreenLoadingBehaviourReference = null;
        public FullScreenLoadingBehaviour FullScreenLoadingBehaviourReference
        {
            get => _fullScreenLoadingBehaviourReference;
            set => _fullScreenLoadingBehaviourReference = value;
        }

        [Title("Toast")] /*//////////////////////////////////////////*/
        [SerializeField, Min(1)]
        private int _numberOfToastsToSpawnAtStart = 3;
        public int NumberOfToastsToSpawnAtStart => _numberOfToastsToSpawnAtStart;

        [SerializeField, Min(1)]
        private int _maxConcurrentToasts = 10;
        public int MaxConcurrentToasts => _maxConcurrentToasts;
        
        [SerializeField]
        private Color _toastDefaultBackgroundColor = new Color(0f, 0f, 0f, 0.58f);
        public Color ToastDefaultBackgroundColor => _toastDefaultBackgroundColor;

        [SerializeField]
        private Color _toastDefaultTextColor = Color.white;
        public Color ToastDefaultTextColor => _toastDefaultTextColor;
        
        [SerializeField]
        private float _toastDisappearAnimDuration = 0.23f;
        public float ToastDisappearAnimDuration => _toastDisappearAnimDuration;
        
        [SerializeField]
        private float _toastAppearAnimDuration = 0.35f;
        public float ToastAppearAnimDuration => _toastAppearAnimDuration;
        
        [SerializeField]
        private float _toastAppearDurationOnDisplay = 2.75f;
        public float ToastAppearDurationOnDisplay => _toastAppearDurationOnDisplay;

        [SerializeField]
        private ToastBehaviour _toastBehaviourReference = null;
        public ToastBehaviour ToastBehaviourReference
        {
            get => _toastBehaviourReference;
            set => _toastBehaviourReference = value;
        }

        [Title("Popup")] /*//////////////////////////////////////////*/
        [SerializeField, Min(1)]
        private int _numberOfPopupsToSpawnAtStart = 1;
        public int NumberOfPopupsToSpawnAtStart => _numberOfPopupsToSpawnAtStart;

        [SerializeField]
        private DynamicPopupBehaviour _dynamicPopupBehaviourReference = null;
        public DynamicPopupBehaviour DynamicPopupBehaviourReference
        {
            get => _dynamicPopupBehaviourReference;
            set => _dynamicPopupBehaviourReference = value;
        }
    }
}