using Unity.Netcode;
using UnityEngine.Assertions;

namespace Core.Interaction
{
    public abstract class InteractableBase : NetworkBehaviour
    {
        private NetworkVariable<NetworkBehaviourReference> currentInteractionCompRef = new NetworkVariable<NetworkBehaviourReference>();

        public InteractionComponent CurrentInteractionComponent
        {
            get => currentInteractionCompRef.Value.TryGet<InteractionComponent>(out var interactionComp) ? interactionComp : null;
            set
            {
                if (value != null)
                {
                    currentInteractionCompRef.Value = new NetworkBehaviourReference(value);
                }
                else
                {
                    currentInteractionCompRef.Value = new NetworkBehaviourReference();
                }
            }
        }

        private bool IsInteractoinValid(InteractionComponent interactionComponent)
        {
            //No client prediction
            return IsServer
                && interactionComponent.CurrentInteractable == this
                && CurrentInteractionComponent == interactionComponent;
        }

        public void StartInteraction(InteractionComponent interactionComponent)
        {
            Assert.IsTrue(IsInteractoinValid(interactionComponent));
            if (IsInteractoinValid(interactionComponent))
            {
                StartInteraction_Internal(interactionComponent);
            }
        }

        public void EndInteraction(InteractionComponent interactionComponent)
        {
            Assert.IsTrue(IsInteractoinValid(interactionComponent));
            if (IsInteractoinValid(interactionComponent))
            {
                EndInteraction_Internal(interactionComponent);
            }
        }

        protected abstract void StartInteraction_Internal(InteractionComponent interactionComponent);
        protected abstract void EndInteraction_Internal(InteractionComponent interactionComponent);
    }
}

