using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SpektraGames.ObjectPooling.Runtime;
using SpektraGames.SpektraUtilities.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpektraGames.RuntimeUI.Runtime
{
    public class DynamicPopupBehaviour : UIBehaviourBase<DynamicPopupBehaviour>, IPoolCallbacks
    {
        public override DynamicPopupBehaviour GetBehaviour => this;

        [SerializeField, SetRef("Text_Title")]
        private TMP_Text _titleText = null;

        [SerializeField, SetRef("Text_Body")]
        private TMP_Text _bodyText = null;

        [SerializeField, SetRef("Image_Background")]
        private Button _backgroundButton = null;

        [SerializeField, SetRef("Button_Close")]
        private Button _closeButton = null;

        [SerializeField, SetRef("Button_Reference")]
        private PopupButtonBehaviour _referenceButton = null;

        private List<PopupButtonBehaviour> _spawnedButtons = new List<PopupButtonBehaviour>();

        [ShowInInspector, ReadOnly]
        private PopupBuilder builder;

        private Canvas _sortCanvas = null;

        private void Start()
        {
            _backgroundButton.onClick.AddListener(OnClickBackgroundButton);
            _closeButton.onClick.AddListener(OnClickCloseButton);
        }

        protected override void OnDestroy()
        {
            _backgroundButton.onClick.RemoveListener(OnClickBackgroundButton);
            _closeButton.onClick.RemoveListener(OnClickCloseButton);

            base.OnDestroy();
        }

        public void Show(PopupBuilder popupBuilder, short overriddenSortOrder = -1)
        {
            this.builder = popupBuilder;

            if (_titleText != null)
            {
                _titleText.text = builder.TitleText;
            }

            if (_bodyText != null)
            {
                _bodyText.text = builder.BodyText;
            }

            SetButtons();

            ActivateContent();

            if (overriddenSortOrder != -1)
            {
                _sortCanvas = gameObject.GetOrAddComponent<Canvas>();
                _sortCanvas.overrideSorting = true;
                _sortCanvas.sortingOrder = overriddenSortOrder;
            }
        }

        private void SetButtons()
        {
            if (!_spawnedButtons.Contains(_referenceButton))
                _spawnedButtons.Add(_referenceButton);

            // Disable all action buttons
            for (var i = 0; i < _spawnedButtons.Count; i++)
            {
                _spawnedButtons[i].gameObject.SetActive(false);
            }

            // Create or edit current action buttons
            if (builder.ActionButtons != null)
            {
                int i = 0;
                foreach (var builderActionButton in builder.ActionButtons)
                {
                    string buttonText = builderActionButton.Key;
                    Action action = builderActionButton.Value;

                    if (!_spawnedButtons.HaveIndex(i))
                    {
                        // Instantiate new button
                        _spawnedButtons.Add(Instantiate<PopupButtonBehaviour>(_referenceButton,
                            _referenceButton.transform.parent));
                    }

                    _spawnedButtons[i].SetButton(buttonText, action);

                    // int _index = i;
                    _spawnedButtons[i].Button.onClick.AddListener(() =>
                    {
                        if (builder.ActionButtons.TryGetValue(buttonText, out Action actionFounded))
                        {
                            actionFounded?.Invoke();

                            if (builder.AutoCloseOnAnyActionButtonPress)
                            {
                                if (InternalCommonClose())
                                {
                                    TriggerPopupClosed(PopupBuilder.PopupCloseType.AutoClosed);
                                }
                            }
                        }
                    });

                    _spawnedButtons[i].gameObject.SetActive(true);

                    i++;
                }
            }

            // Other
            if (_closeButton != null)
            {
                _closeButton.gameObject.SetActive(builder.ActivateCloseButton);
            }
        }

        private void OnClickBackgroundButton()
        {
            if (builder.CloseWhenBackgroundPress)
            {
                if (InternalCommonClose())
                {
                    TriggerPopupClosed(PopupBuilder.PopupCloseType.ClosedByUserClick);
                }
            }
        }

        private void OnClickCloseButton()
        {
            if (builder.ActivateCloseButton)
            {
                if (InternalCommonClose())
                {
                    TriggerPopupClosed(PopupBuilder.PopupCloseType.ClosedByUserClick);
                }
            }
        }

        public void Close()
        {
            if (InternalCommonClose())
            {
                TriggerPopupClosed(PopupBuilder.PopupCloseType.ClosedByCode);
            }
        }

        private bool InternalCommonClose()
        {
            if (!IsActive)
                return false;

            Release();

            return true;
        }

        private void Release()
        {
            // Disable all action buttons
            for (var i = 0; i < _spawnedButtons.Count; i++)
            {
                _spawnedButtons[i].Button.onClick.RemoveAllListeners();
            }

            DeactivateContent();
            IndependentPoolManager.Return(gameObject, RuntimeUI._popupsPoolId);
        }

        private void TriggerPopupClosed(PopupBuilder.PopupCloseType cause)
        {
            if (builder.OnPopupClosedCallback != null)
            {
                builder.OnPopupClosedCallback?.Invoke(cause);

                Delegate[] listeners = builder.OnPopupClosedCallback.GetInvocationList();
                for (var i = 0; i < listeners.Length; i++)
                {
                    if (listeners[i] != null)
                        builder.OnPopupClosedCallback -= listeners[i] as PopupBuilder.OnPopupClosedDel;
                }

                builder.OnPopupClosedCallback = null;
            }

            builder = default;

            if (_sortCanvas)
            {
                Destroy(_sortCanvas);
                _sortCanvas = null;
            }
        }

        internal static void CloseAllPopups()
        {
            var activeBehaviours = ActiveBehaviours.ToList();

            for (var i = 0; i < activeBehaviours.Count; i++)
            {
                if (activeBehaviours[i] == null)
                    continue;

                if (activeBehaviours[i].InternalCommonClose())
                {
                    activeBehaviours[i].TriggerPopupClosed(PopupBuilder.PopupCloseType.ForceClosedAll);
                }
            }
        }

        internal static bool IsThereAnyPopupActive => ActiveBehaviours.Count > 0;

        public void OnGetFromPool()
        {
            transform.SetParent(RuntimeUI.Canvas.transform);
            transform.localScale = Vector3.one;
            (transform as RectTransform).SetLRTB(Vector4.zero);
        }

        public void OnReturnToPool()
        {
        }
    }
}