using Runtime.Network;
using Runtime.Simulation;
using UnityEngine;

namespace Runtime.Client
{
    public class Client : World, IClient
    {
        public LagNetwork ServerNetwork { get; set; }

        public string LocalEntityId { get; set; }

        public bool EnableClientPrediction;

        private void Start()
        {
            var server = GameObject.FindGameObjectWithTag("Server");
            server.GetComponent<IServer>().Connect(this);
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
                var inputMessage = clientMovement.BuildInputMessage(dt);
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
                        var clientMovement = entity.GetComponent<ClientEntityMovement>();
                        clientMovement.ReceiveServerSnapshot(snapshot);
                    }
                }
            }
        }
    }
}
