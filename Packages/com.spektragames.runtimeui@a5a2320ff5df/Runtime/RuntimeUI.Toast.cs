using SpektraGames.ObjectPooling.Runtime;
using UnityEngine;

namespace SpektraGames.RuntimeUI.Runtime
{
    public partial class RuntimeUI
    {
        public static void ShowToast(string message)
        {
            ShowToast(message, null, null);
        }

        public static void ShowToast(string message, Color? backgroundColor = null, Color? textColor = null)
        {
            if (!CanCallRuntimeMethod)
                return;

            ToastBehaviour behaviour = IndependentPoolManager.GetByComponent<ToastBehaviour>(_toastsPoolId);
            behaviour.Show(message, backgroundColor, textColor);

            // Make the loading in front of the all other behaviours
            if (CurrentFullScreenLoadingBehaviour != null)
                CurrentFullScreenLoadingBehaviour.transform.SetAsLastSibling();

            // Make this toast in front of the all other behaviours
            if (behaviour != null)
                behaviour.transform.SetAsLastSibling();

            var activeToasts = ToastBehaviour.CurrentActiveToastBehaviours;
            for (var i = 0; i < activeToasts.Count; i++)
            {
                if(!activeToasts[i])
                    continue;
                
                if(activeToasts[i] == behaviour)
                    continue;
                
                // Force fade out
                activeToasts[i].ForceFadeOut();
            }
        }

        public static void ForceHideAllToasts()
        {
            if (!CanCallRuntimeMethod)
                return;

            ToastBehaviour.HideAllToasts();
        }
    }
}