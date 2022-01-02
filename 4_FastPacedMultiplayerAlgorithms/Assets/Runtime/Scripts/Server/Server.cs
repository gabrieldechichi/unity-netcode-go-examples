using System;
using System.Collections.Generic;
using Runtime.Simulation;
using UnityEngine;
using UnityEngine.Assertions;

namespace Runtime.Server
{

    public class Server : World, IServer
    {
        [SerializeField] private NetworkEntity playerPrefab;
        private List<IClient> clients = new List<IClient>();

        public void Connect(IClient newClient)
        {
            var entityId = Guid.NewGuid().ToString();
            //TODO: send network message
            newClient.ServerNetwork = Network;
            newClient.LocalEntityId = entityId;

            SpawnEntity(playerPrefab, entityId, EntityNetworkRole.Server);

            foreach (var client in clients)
            {
                client.SpawnEntity(playerPrefab, entityId, EntityNetworkRole.Ghost);
            }

            newClient.SpawnEntity(playerPrefab, entityId, EntityNetworkRole.OwningClient);

            foreach (var client in clients)
            {
                var success = entities.TryGetValue(client.LocalEntityId, out var entity);
                Assert.IsTrue(success, $"Couldn't find entity for id {client.LocalEntityId}");
                if (success)
                {
                    newClient.SpawnEntity(entity, entity.EntityId, EntityNetworkRole.Ghost);
                }
            }

            clients.Add(newClient);
        }

        protected override void UpdateWorld(float dt)
        {
            //TODO: Configurable update rate
            ProcessMessages();
            SendSnapshot();
        }

        private void ProcessMessages()
        {
            foreach (var msg in Network.Receive<MovementInput>())
            {
                if (entities.TryGetValue(msg.EntityId, out var entity))
                {
                    var serverMovement = entity.GetComponent<ServerEntityMovement>();
                    serverMovement.Move(msg);
                }
            }
        }

        private void SendSnapshot()
        {
            var snapshots = new EntitySnapshots
            {
                Snapshots = new List<EntitySnapshot>(entities.Count)
            };

            foreach (var entity in entities.Values)
            {
                var serverMovement = entity.GetComponent<ServerEntityMovement>();
                snapshots.Snapshots.Add(serverMovement.GenerateSnapshot());
            }

            foreach (var client in clients)
            {
                client.Network.Send(snapshots);
            }
        }
    }
}
