using Unity.Netcode;
using UnityEngine;

namespace Core.Interaction
{
    public class InteractionComponent : NetworkBehaviour
    {
        [SerializeField] private float interactionRadius;
        [SerializeField] private float interactionDistance;

        [SerializeField] private LayerMask interactionMask = -1;

        private Vector3 InteractionCenter => transform.position + transform.forward * interactionDistance;

        private Collider[] overlapColliders = new Collider[5];
        private NetworkVariable<NetworkBehaviourReference> currentInteractableRef = new NetworkVariable<NetworkBehaviourReference>();

        public InteractableBase CurrentInteractable
        {
            get => currentInteractableRef.Value.TryGet<InteractableBase>(out var interactable) ? interactable : null;
            private set
            {
                if (value != null)
                {
                    currentInteractableRef.Value = new NetworkBehaviourReference(value);
                }
                else
                {
                    currentInteractableRef.Value = new NetworkBehaviourReference();
                }
            }
        }

        public void TryInteract()
        {
            if (CurrentInteractable == null)
            {
                var interactable = FindFirstInteractable();
                if (interactable != null)
                {
                    StartInteraction_ServerRpc(interactable);
                }
            }
            else
            {
                EndInteraction();
            }
        }

        public void EndInteraction()
        {
            EndInteraction_ServerRpc(CurrentInteractable);
        }

        [ServerRpc(RequireOwnership = true)]
        private void EndInteraction_ServerRpc(NetworkBehaviourReference interactableRef)
        {
            if (interactableRef.TryGet<InteractableBase>(out var interactable)
                && interactable == CurrentInteractable
                && interactable.CurrentInteractionComponent == this)
            {
                CurrentInteractable.EndInteraction(this);
                CurrentInteractable.CurrentInteractionComponent = null;
                CurrentInteractable = null;
            }
        }

        [ServerRpc(RequireOwnership = true)]
        private void StartInteraction_ServerRpc(NetworkBehaviourReference interactableRef)
        {
            bool CanInteractWith(InteractableBase interactable)
            {
                const float rangeToleranceMultiplier = 2.0f;
                bool isInRange = (InteractionCenter - interactable.transform.position).sqrMagnitude
                    < interactionRadius * interactionRadius * rangeToleranceMultiplier;

                return interactable.CurrentInteractionComponent == null && isInRange;
            }

            if (CurrentInteractable == null)
            {
                if (interactableRef.TryGet<InteractableBase>(out var interactable)
                    && CanInteractWith(interactable))
                {
                    CurrentInteractable = interactable;
                    interactable.CurrentInteractionComponent = this;
                    CurrentInteractable.StartInteraction(this);
                }
            }
        }

        private InteractableBase FindFirstInteractable()
        {
            var hitCount = Physics.OverlapSphereNonAlloc(InteractionCenter, interactionRadius, overlapColliders, interactionMask);
            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; i++)
                {
                    if (overlapColliders[i].TryGetComponent<InteractableBase>(out var interactable))
                    {
                        return interactable;
                    }
                }
            }
            return null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(InteractionCenter, interactionRadius);
        }
    }
}

