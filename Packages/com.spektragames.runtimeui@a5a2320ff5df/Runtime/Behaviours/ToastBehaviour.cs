using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DG.Tweening;
using SpektraGames.ObjectPooling.Runtime;
using SpektraGames.SpektraUtilities.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SpektraGames.RuntimeUI.Runtime
{
    public class ToastBehaviour : UIBehaviourBase<ToastBehaviour>, IPoolCallbacks
    {
        internal static List<ToastBehaviour> CurrentActiveToastBehaviours = new List<ToastBehaviour>();

        public override ToastBehaviour GetBehaviour => this;

        [SerializeField, SetRef("AnimationStartPosition", typeof(RectTransform))]
        private RectTransform _animationStartPosition = null;

        [SerializeField, SetRef("AnimationStandPosition", typeof(RectTransform))]
        private RectTransform _animationStandPosition = null;

        [SerializeField, SetRef("AnimationEndPosition", typeof(RectTransform))]
        private RectTransform _animationEndPosition = null;

        [SerializeField, SetRef("Handle", typeof(RectTransform))]
        private RectTransform _handle = null;

        [SerializeField, SetRef("Handle")]
        private Image _backgroundImage = null;

        [SerializeField, SetRef("Handle", typeof(CanvasGroup))]
        private CanvasGroup _canvasGroup = null;

        [SerializeField, SetRef("Text_Message")]
        private TMP_Text _messageText = null;

        private Color DefaultBackgroundColor => RuntimeUISettings.Instance.ToastDefaultBackgroundColor;
        private Color DefaultTextColor => RuntimeUISettings.Instance.ToastDefaultTextColor;
        private float DisappearAnimDuration => RuntimeUISettings.Instance.ToastDisappearAnimDuration;
        private float AppearAnimDuration => RuntimeUISettings.Instance.ToastAppearAnimDuration;
        private float AppearDurationOnDisplay => RuntimeUISettings.Instance.ToastAppearDurationOnDisplay;

        private Sequence _sequence = null;
        private Tween _forceFadeTween = null;

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += SceneManagerOnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= SceneManagerOnSceneUnloaded;
        }

        internal void Show(string message, Color? backgroundColor = null, Color? textColor = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogError("You can't pass null or empty message");
                Release();
                return;
            }

            Color backgroundColorLocal = backgroundColor == null ? DefaultBackgroundColor : backgroundColor.Value;
            Color textColorLocal = textColor == null ? DefaultTextColor : textColor.Value;

            if (this._backgroundImage.color != backgroundColor)
                this._backgroundImage.color = backgroundColorLocal;
            if (this._messageText.color != textColorLocal)
                this._messageText.color = textColorLocal;

            this._messageText.text = message;
            _canvasGroup.alpha = 0f;

            KillSequence();
            ActivateContent();

            _handle.anchoredPosition = _animationStartPosition.anchoredPosition;

            // Animation
            _sequence = DOTween.Sequence();
            _sequence.Append(_handle.DOAnchorPos(_animationStandPosition.anchoredPosition, AppearAnimDuration));
            _sequence.Join(_canvasGroup.DOFade(1f, AppearAnimDuration + 0.1f));
            _sequence.AppendInterval(AppearDurationOnDisplay);
            _sequence.Append(_handle.DOAnchorPos(_animationEndPosition.anchoredPosition, DisappearAnimDuration));
            _sequence.Join(_canvasGroup.DOFade(0f, DisappearAnimDuration - 0.1f));
            _sequence.SetUpdate(true);
            _sequence.Play();
            _sequence.OnComplete(Release);

            if (!CurrentActiveToastBehaviours.Contains(this))
                CurrentActiveToastBehaviours.Add(this);
        }

        protected override void OnDestroy()
        {
            KillSequence();
            base.OnDestroy();

            if (CurrentActiveToastBehaviours.Contains(this))
                CurrentActiveToastBehaviours.Remove(this);
        }

        public void ForceFadeOut()
        {
            if (_sequence != null && _forceFadeTween == null)
            {
                KillSequence();
                _forceFadeTween = _canvasGroup.DOFade(0f, DisappearAnimDuration).OnComplete(() =>
                {
                    _forceFadeTween = null;
                    Release();
                });
            }
        }

        private void Release()
        {
            if (CurrentActiveToastBehaviours.Contains(this))
                CurrentActiveToastBehaviours.Remove(this);

            KillSequence();
            DeactivateContent();

            if (_forceFadeTween != null)
            {
                _forceFadeTween.Kill(false);
                _forceFadeTween = null;
            }

            IndependentPoolManager.Return(gameObject, RuntimeUI._toastsPoolId);
        }

        private void KillSequence()
        {
            if (_sequence != null)
            {
                _sequence.Kill(false);
                _sequence = null;
            }
        }

        private void SceneManagerOnSceneUnloaded(Scene scene)
        {
            Release();
        }

        internal static void HideAllToasts()
        {
            var activeBehaviours = ActiveBehaviours.ToList();

            for (var i = 0; i < activeBehaviours.Count; i++)
            {
                if (activeBehaviours[i] == null)
                    continue;

                activeBehaviours[i].Release();
            }
        }

        public void OnGetFromPool()
        {
            if (_forceFadeTween != null)
            {
                _forceFadeTween.Kill(false);
                _forceFadeTween = null;
            }
            
            transform.SetParent(RuntimeUI.Canvas.transform);
            transform.localScale = Vector3.one;
            (transform as RectTransform).SetLRTB(Vector4.zero);
            KillSequence();
        }

        public void OnReturnToPool()
        {
            if (_forceFadeTween != null)
            {
                _forceFadeTween.Kill(false);
                _forceFadeTween = null;
            }
        }
    }
}