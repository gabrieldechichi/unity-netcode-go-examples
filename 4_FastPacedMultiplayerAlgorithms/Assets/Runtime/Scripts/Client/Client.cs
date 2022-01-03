using Runtime.Network;
using Runtime.Simulation;
using UnityEngine;

namespace Runtime.Client
{
    public enum ClientInputType
    {
        None, WASD, ArrowKeys
    }

    public class Client : World, IClient
    {
        public LagNetwork ServerNetwork { get; set; }

        public string LocalEntityId { get; set; }

        public Color playerColor = Color.red;

        public bool EnableClientPrediction;
        public bool EnableServerReconciliation;

        public ClientInputType inputType;

        private void Start()
        {
            var server = GameObject.FindGameObjectWithTag("Server");
            server.GetComponent<IServer>().Connect(this);

            //TODO (hack): We don't really support replication of anything other than movement
            //And our network is fake anyway
            var worlds = FindObjectsOfType<World>();
            foreach (var world in worlds)
            {
                var entity = world.GetEntity(LocalEntityId);
                var color = playerColor;
                color.a = entity.Role == EntityNetworkRole.OwningClient ? 1.0f : 0.5f;
                entity.GetComponent<SpriteRenderer>().color = color;
            }
        }

        protected override void UpdateWorld(float dt)
        {
            ProcessMessages();
            ProcessLocalInput(dt);
        }

        private void ProcessLocalInput(float dt)
        {
            if (entities.TryGetValue(LocalEntityId, out var localEntity))
            {
                var clientMovement = localEntity.GetComponent<ClientEntityMovement>();
                var inputMessage = clientMovement.BuildInputMessage(dt, inputType);
                ServerNetwork.Send(inputMessage);

                if (EnableClientPrediction)
                {
                    clientMovement.Move(inputMessage);
                }
            }
        }

        private void ProcessMessages()
        {
            foreach (var msg in Network.Receive<EntitySnapshots>())
            {
                foreach (var snapshot in msg.Snapshots)
                {
                    if (entities.TryGetValue(snapshot.EntityId, out var entity))
                    {
                        if (entity.EntityId == LocalEntityId)
                        {
                            var clientMovement = entity.GetComponent<ClientEntityMovement>();
                            clientMovement.ReceiveServerSnapshot(snapshot, EnableServerReconciliation);
                        }
                        else
                        {
                            var ghostMovement = entity.GetComponent<GhostEntityMovement>();
                            ghostMovement.ReceiveServerSnapshot(snapshot);
                        }
                    }
                }
            }
        }
    }
}
