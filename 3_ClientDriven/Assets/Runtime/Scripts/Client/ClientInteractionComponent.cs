using ClientDriven.Common;
using ClientDriven.Server;
using UnityEngine;

namespace ClientDriven.Client
{
    [RequireComponent(typeof(ServerInteractionComponent))]
    public class ClientInteractionComponent : ClientBehaviour
    {
        [SerializeField] private float interactionCenterDistance;
        [SerializeField] private float interactionRadius;
        [SerializeField] private LayerMask interactMask = -1;

        private Collider[] colliders = new Collider[5];

        private Vector3 InteractCenter => transform.position + transform.forward * interactionCenterDistance;

        private ServerInteractionComponent serverInteraction;
        public ServerInteractionComponent ServerInteraction => serverInteraction == null
            ? serverInteraction = GetComponent<ServerInteractionComponent>()
            : serverInteraction;

        public bool CanInteract => ServerInteraction.CanInteract;

        public void TryInteract()
        {
            if (ServerInteraction.CurrentInteractable == null)
            {
                var hitCount = Physics.OverlapSphereNonAlloc(InteractCenter, interactionRadius, colliders, interactMask);
                if (hitCount > 0)
                {
                    for (int i = 0; i < hitCount; i++)
                    {
                        if (colliders[i].TryGetComponent<ClientInteractable>(out var interactable))
                        {
                            interactable.OnInteract(this);
                            break;
                        }
                    }
                }
            }
            else
            {
                var interactable = ServerInteraction.GetCurrentInteractableComponent<ClientInteractable>();
                if (interactable != null)
                {
                    interactable.OnInteract(this);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(InteractCenter, interactionRadius);
        }
    }

}