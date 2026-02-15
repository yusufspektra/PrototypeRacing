using UnityEngine;

namespace SpektraGames.RuntimeUI.Runtime
{
    public partial class RuntimeUI
    {
        private static FullScreenLoadingBehaviour CurrentFullScreenLoadingBehaviour = null;

        public static bool IsLoadingActive =>
            CurrentFullScreenLoadingBehaviour != null && CurrentFullScreenLoadingBehaviour.IsActive;

        public static void ShowLoading(string loadingText = "", short overriddenSortOrder = -1)
        {
            if (!CanCallRuntimeMethod)
                return;

            CurrentFullScreenLoadingBehaviour?.Show(loadingText, overriddenSortOrder);
            
            // Make the loading in front of the all other behaviours
            if (CurrentFullScreenLoadingBehaviour != null)
                CurrentFullScreenLoadingBehaviour.transform.SetAsLastSibling();
        }

        public static void HideLoading(string loadingText = "")
        {
            if (!CanCallRuntimeMethod)
                return;

            if (IsLoadingActive)
                CurrentFullScreenLoadingBehaviour?.Hide();
        }
    }
}