using Unity.Netcode;
using UnityEngine.Assertions;

namespace Core.Interaction
{
    public abstract class InteractableBase : NetworkBehaviour
    {

        public void StartInteraction(InteractionComponent interactionComponent)
        {
            //No client prediction
            Assert.IsTrue(IsServer);
            if (IsServer && interactionComponent.CurrentInteractable == this)
            {
                StartInteraction_Internal(interactionComponent);
            }
        }

        public void EndInteraction(InteractionComponent interactionComponent)
        {
            //No Client prediction
            Assert.IsTrue(IsServer);
            if (IsServer && interactionComponent.CurrentInteractable == this)
            {
                EndInteraction_Internal(interactionComponent);
            }
        }

        protected abstract void StartInteraction_Internal(InteractionComponent interactionComponent);
        protected abstract void EndInteraction_Internal(InteractionComponent interactionComponent);
    }
}

