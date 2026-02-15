using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using SpektraGames.SpektraUtilities.Runtime;
using TMPro;
using UnityEngine;

namespace SpektraGames.RuntimeUI.Runtime
{
    public class FullScreenLoadingBehaviour : UIBehaviourBase<FullScreenLoadingBehaviour>
    {
        public override FullScreenLoadingBehaviour GetBehaviour => this;

        [SerializeField]
        private TMP_Text _loadingText = null;

        private Tween loadingTextTween = null;
        private string _cachedLoadingText = null;
        private bool _useDotAnimation = false;

        private Canvas _sortCanvas = null;

        protected override void Awake()
        {
            base.Awake();
            _useDotAnimation = RuntimeUISettings.Instance.UseDotAnimationForLoading;
        }

        internal void Show(string loadingText = null, short overriddenSortOrder = -1)
        {
            StopAnimation();
            ActivateContent();
            StartAnimation(loadingText);

            if (overriddenSortOrder != -1)
            {
                _sortCanvas = gameObject.GetOrAddComponent<Canvas>();
                _sortCanvas.overrideSorting = true;
                _sortCanvas.sortingOrder = overriddenSortOrder;
            }
        }

        internal void Hide()
        {
            StopAnimation();
            DeactivateContent();

            if (_sortCanvas)
            {
                Destroy(_sortCanvas);
                _sortCanvas = null;
            }
        }

        protected override void OnDestroy()
        {
            StopAnimation();
            base.OnDestroy();
        }

        private void StartAnimation(string loadingText = null)
        {
            if (string.IsNullOrEmpty(loadingText))
            {
                _loadingText?.SetText("");
                _cachedLoadingText = null;
                return;
            }

            if (!_useDotAnimation)
            {
                _cachedLoadingText = loadingText.ToString();
                _loadingText?.SetText(_cachedLoadingText);
                return;
            }

            _cachedLoadingText = loadingText.ToString();

            while (_cachedLoadingText.EndsWith("."))
            {
                _cachedLoadingText = _cachedLoadingText.Remove(_cachedLoadingText.Length - 1);
            }

            if (string.IsNullOrEmpty(_cachedLoadingText))
            {
                _loadingText?.SetText("");
                _cachedLoadingText = null;
                return;
            }

            _loadingText?.SetText(_cachedLoadingText);

            float animDuration = 0.50f;
            int dotCount = 0;
            float t = 0;
            loadingTextTween = DOTween.To(
                    () => t, (_val) => t = _val, 1f, animDuration)
                .OnStepComplete(() =>
                {
                    dotCount += 1;
                    if (dotCount >= 3)
                    {
                        dotCount = 0;
                    }

                    if (dotCount == 1)
                        _loadingText.text = _cachedLoadingText + ".";
                    else if (dotCount == 2)
                        _loadingText.text = _cachedLoadingText + "..";
                    else if (dotCount == 3)
                        _loadingText.text = _cachedLoadingText + "...";
                    else
                    {
                        _loadingText.text = _cachedLoadingText;
                    }
                })
                .SetUpdate(true)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopAnimation()
        {
            if (loadingTextTween != null)
            {
                loadingTextTween.Kill();
                loadingTextTween = null;
            }
        }
    }
}