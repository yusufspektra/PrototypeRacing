using SpektraGames.ObjectPooling.Runtime;
using UnityEngine;

namespace SpektraGames.RuntimeUI.Runtime
{
    public partial class RuntimeUI
    {
        public static DynamicPopupBehaviour ShowPopup(PopupBuilder builder, short overriddenSortOrder = -1)
        {
            if (!CanCallRuntimeMethod)
                return null;

            DynamicPopupBehaviour behaviour =
                IndependentPoolManager.GetByComponent<DynamicPopupBehaviour>(_popupsPoolId);
            behaviour.Show(builder, overriddenSortOrder);

            // Make the loading in front of the all other behaviours
            if (CurrentFullScreenLoadingBehaviour != null)
                CurrentFullScreenLoadingBehaviour.transform.SetAsLastSibling();

            return behaviour;
        }

        public static void ForceCloseAllPopups()
        {
            if (!CanCallRuntimeMethod)
                return;

            DynamicPopupBehaviour.CloseAllPopups();
        }

        public static bool IsThereAnyPopupActive => DynamicPopupBehaviour.IsThereAnyPopupActive;
    }
}