using ClientDriven.Server;
using UnityEngine;

namespace ClientDriven.Client
{

    [RequireComponent(typeof(ServerIngredient))]
    public class ClientIngredient : ClientInteractable
    {
        ServerIngredient serverIngredient;
        ServerIngredient ServerIngredient => serverIngredient == null
            ? serverIngredient = GetComponent<ServerIngredient>()
            : serverIngredient;

        public override void OnInteract(ClientInteractionComponent interactionComp)
        {
            if (interactionComp.ServerInteraction.CurrentInteractable != NetworkObject)
            {
                ServerIngredient.GetPickedBy(interactionComp.ServerInteraction);
                ServerIngredient.GetPicketBy_ServerRpc(interactionComp.ServerInteraction);
            }
            else
            {
                ServerIngredient.Release(interactionComp.ServerInteraction);
                serverIngredient.Release_ServerRpc(interactionComp.ServerInteraction);
            }
        }
    }
}