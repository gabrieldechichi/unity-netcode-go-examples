using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Runtime.Simulation
{
    public class Server : World
    {
        [SerializeField] private NetworkEntity playerPrefab;
        private List<Client> clients = new List<Client>();

        public void Connect(Client newClient)
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
            ProcessMessages(dt);
            SendSnapshot();
        }

        private void ProcessMessages(float dt)
        {
            foreach (var msg in Network.Receive<MovementInput>())
            {
                if (entities.TryGetValue(msg.EntityId, out var entity))
                {
                    entity.Server_ProcessMovementInput(msg, dt);
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
                snapshots.Snapshots.Add(entity.Server_GenerateSnapshot());
            }

            foreach (var client in clients)
            {
                client.Network.Send(snapshots);
            }
        }
    }
}
