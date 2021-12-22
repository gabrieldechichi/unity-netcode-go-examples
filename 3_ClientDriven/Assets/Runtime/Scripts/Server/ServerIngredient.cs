using ClientDriven.Common;
using Unity.Netcode;
using UnityEngine;

namespace ClientDriven.Server
{
    public enum IngredientType
    {
        Red,
        Blue,
        Purple,
        Max
    }

    [RequireComponent(typeof(Rigidbody))]
    public class ServerIngredient : ServerBehaviour
    {
        [SerializeField] private NetworkVariable<IngredientType> ingredientType;

        [SerializeField] private Vector3 releaseDir = Vector3.forward;
        [SerializeField] private float releaseForce = 10;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        [ServerRpc]
        public void GetPicketBy_ServerRpc(NetworkBehaviourReference networkBehaviourRef)
        {
            if (networkBehaviourRef.TryGet<ServerInteractionComponent>(out var interactionComp))
            {
                GetPickedBy(interactionComp);
            }
        }

        [ServerRpc]
        public void Release_ServerRpc(NetworkBehaviourReference networkBehaviourRef)
        {
            if (networkBehaviourRef.TryGet<ServerInteractionComponent>(out var interactionComp))
            {
                Release(interactionComp);
            }
        }

        public void GetPickedBy(ServerInteractionComponent interactionComp)
        {
            if (interactionComp.CurrentInteractable == null)
            {
                rb.isKinematic = true;
                interactionComp.CurrentInteractable = NetworkObject;
                transform.SetParent(interactionComp.transform);
                transform.localPosition = new Vector3(0, 1, 0);
            }
        }

        public void Release(ServerInteractionComponent interactionComp)
        {
            if (interactionComp.CurrentInteractable == NetworkObject)
            {
                interactionComp.CurrentInteractable = null;
                transform.SetParent(null);
                rb.isKinematic = false;
                rb.AddForce(releaseDir.normalized * releaseForce, ForceMode.Impulse);
            }
        }
    }
}
