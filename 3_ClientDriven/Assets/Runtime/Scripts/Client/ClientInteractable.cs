using ClientDriven.Common;

namespace ClientDriven.Client
{
    public abstract class ClientInteractable : ClientBehaviour
    {
        public abstract void OnInteract(ClientInteractionComponent interactionComp);
    }
}