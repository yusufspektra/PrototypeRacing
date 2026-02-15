using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace SpektraGames.RuntimeUI.Runtime
{
    [System.Serializable]
    public struct PopupBuilder
    {
        public enum PopupCloseType
        {
            ClosedByUserClick = 0, // User clicked to close or background(if enabled CloseWhenBackgroundPress) button
            ClosedByCode = 1, // Trigger if you call DynamicPopupBehaviour.Close()
            ForceClosedAll = 2, // Trigger if you call RuntimeUI.ForceCloseAllPopups()
            AutoClosed = 3, // Trigger if user click an action button when AutoCloseOnAnyActionButtonPress is true
        }

        [ShowInInspector, OdinSerialize] public string TitleText { get; set; }
        [ShowInInspector, OdinSerialize] public string BodyText { get; set; }
        [ShowInInspector, OdinSerialize] public bool AutoCloseOnAnyActionButtonPress { get; set; }
        [ShowInInspector, OdinSerialize] public bool ActivateCloseButton { get; set; }
        [ShowInInspector, OdinSerialize] public bool CloseWhenBackgroundPress { get; set; }
        [ShowInInspector, OdinSerialize] public Dictionary<string, Action> ActionButtons { get; set; }

        public delegate void OnPopupClosedDel(PopupCloseType closeCause);

        internal OnPopupClosedDel OnPopupClosedCallback { get; set; }

        public static PopupBuilder Build(
            string titleText,
            string bodyText,
            bool autoCloseOnAnyActionButtonPress = true,
            bool activateCloseButton = false,
            bool closeWhenBackgroundPress = false
        )
        {
            return new PopupBuilder().BuildInternal(
                titleText,
                bodyText,
                autoCloseOnAnyActionButtonPress,
                activateCloseButton,
                closeWhenBackgroundPress);
        }

        private PopupBuilder BuildInternal(string titleText,
            string bodyText,
            bool autoCloseOnAnyActionButtonPress = true,
            bool activateCloseButton = false,
            bool closeWhenBackgroundPress = false)
        {
            TitleText = titleText;
            BodyText = bodyText;
            AutoCloseOnAnyActionButtonPress = autoCloseOnAnyActionButtonPress;
            ActivateCloseButton = activateCloseButton;
            CloseWhenBackgroundPress = closeWhenBackgroundPress;
            ActionButtons = new Dictionary<string, Action>();
            OnPopupClosedCallback = null;

            return this;
        }

        public void SetPopupClosedCallback(OnPopupClosedDel callback)
        {
            this.OnPopupClosedCallback = callback;
        }

        public void AddActionButton(string buttonText, Action onClick)
        {
            if (ActionButtons == null)
                ActionButtons = new Dictionary<string, Action>();

            ActionButtons[buttonText] = onClick;
        }

        [Button]
        public DynamicPopupBehaviour Show(short overriddenSortOrder = -1)
        {
            if (string.IsNullOrEmpty(TitleText))
            {
                Debug.LogError(new NullReferenceException(nameof(TitleText)).ToString());
                return null;
            }

            if (string.IsNullOrEmpty(BodyText))
            {
                Debug.LogError(new NullReferenceException(nameof(BodyText)).ToString());
                return null;
            }

            return RuntimeUI.ShowPopup(this, overriddenSortOrder);
        }
    }
}