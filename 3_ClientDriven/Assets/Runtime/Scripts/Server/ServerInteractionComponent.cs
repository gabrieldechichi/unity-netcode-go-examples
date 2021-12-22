using ClientDriven.Common;
using Unity.Netcode;
using UnityEngine;

namespace ClientDriven.Server
{
    public class ServerInteractionComponent : ServerBehaviour
    {
        private NetworkVariable<NetworkObjectReference> currentInteractable;

        public NetworkObject CurrentInteractable
        {
            get => currentInteractable.Value.TryGet(out var obj) ? obj : null;
            set => currentInteractable.Value = value != null
                ? new NetworkObjectReference(value)
                : new NetworkObjectReference();
        }

        public T GetCurrentInteractableComponent<T>() where T : MonoBehaviour
        {
            if (CurrentInteractable != null)
            {
                return CurrentInteractable.GetComponent<T>();
            }
            return null;
        }

        public bool CanInteract => CurrentInteractable == null;
    }
}
