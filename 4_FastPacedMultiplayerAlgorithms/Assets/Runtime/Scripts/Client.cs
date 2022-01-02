using Runtime.Network;
using UnityEngine;

namespace Runtime.Simulation
{
    public class Client : World
    {
        [HideInInspector] public LagNetwork ServerNetwork;

        [HideInInspector] public string LocalEntityId;

        public bool EnableClientPrediction;

        private void Start()
        {
            FindObjectOfType<Server>().Connect(this);
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
                var inputMessage = localEntity.Client_ProcessInput(dt);
                ServerNetwork.Send(inputMessage);

                if (EnableClientPrediction)
                {
                    localEntity.Client_PredictMovement(inputMessage);
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
                        entity.Client_ReceiveServerSnapshot(snapshot);
                    }
                }
            }
        }
    }
}
