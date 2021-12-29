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
        private NetworkVariable<NetworkBehaviourReference> currentInteractableRef;

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
            if (IsServer)
            {
                ExecuteInteraction();
            }
            else
            {
                ExecuteInteraction_ServerRpc();
            }
        }

        private void ExecuteInteraction()
        {
            if (CurrentInteractable == null)
            {
                var interactable = FindFirstInteractable();
                if (interactable != null)
                {
                    CurrentInteractable = interactable;
                    CurrentInteractable.StartInteraction(this);
                }
            }
            else
            {
                CurrentInteractable.EndInteraction(this);
                CurrentInteractable = null;
            }
        }

        [ServerRpc(RequireOwnership = true)]
        private void ExecuteInteraction_ServerRpc()
        {
            if (IsServer)
            {
                TryInteract();
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

