using UnityEngine;

namespace SpektraGames.RuntimeUI.Runtime
{
    public class BlockScreenBehaviour : UIBehaviourBase<BlockScreenBehaviour>
    {
        public override BlockScreenBehaviour GetBehaviour => this;
        
        internal void Show()
        {
            ActivateContent();
        }

        internal void Hide()
        {
            DeactivateContent();
        }
    }
}
