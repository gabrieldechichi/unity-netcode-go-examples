using System;
using System.Collections.Generic;
using UnityEngine;

// SnapshotSystem stores:
//
// - Spawn, Despwan commands (done)
// - NetworkVariable value updates (todo)
// - RPC commands (todo)
//
// and sends a SnapshotDataMessage every tick containing all the un-acknowledged commands.
//
// SnapshotSystem can function even if some messages are lost. It provides eventual consistency.
// The client receiving a message will get a consistent state for a given tick, but possibly not every ticks
// Reliable RPCs will be guaranteed, unreliable ones not
//
// SnapshotSystem relies on the Transport adapter to fragment an arbitrary-sized message into packets
// This comes with a tradeoff. The Transport-level fragmentation is specialized for networking
// but lacks the context that SnapshotSystem has of the meaning of the RPC, Spawns, etc...
// This could be revisited in the future
//
// It also relies on the INetworkMessage interface and MessagingSystem, but deals directly
// with the FastBufferReader and FastBufferWriter to read/write the messages

namespace Unity.Netcode
{
    /// <summary>
    /// Header information for a SnapshotDataMessage
    /// </summary>
    internal struct SnapshotHeader
    {
        internal int CurrentTick; // the tick this captures information for
        internal int LastReceivedSequence; // what we are ack'ing
        internal int SpawnCount; // number of spawn commands included
        internal int DespawnCount; // number of despawn commands included
    }

    /// <summary>
    /// A command to despawn an object
    /// Which object it is, and the tick at which it was despawned
    /// </summary>
    internal struct SnapshotDespawnCommand
    {
        // identity
        internal ulong NetworkObjectId;

        // snapshot internal
        internal int TickWritten;
    }

    /// <summary>
    /// A command to spawn an object
    /// Which object it is, what type it has, the spawn parameters and the tick at which it was spawned
    /// </summary>
    internal struct SnapshotSpawnCommand
    {
        // identity
        internal ulong NetworkObjectId;

        // archetype
        internal uint GlobalObjectIdHash;
        internal bool IsSceneObject; //todo: how is this unused ?

        // parameters
        internal bool IsPlayerObject;
        internal ulong OwnerClientId;
        internal ulong ParentNetworkId;
        internal Vector3 ObjectPosition;
        internal Quaternion ObjectRotation;
        internal Vector3 ObjectScale; //todo: how is this unused ?

        // internal
        internal int TickWritten;
    }

    /// <summary>
    /// Stores supplemental meta-information about a Spawn or Despawn command.
    /// This part doesn't get sent, so is stored elsewhere in order to allow writing just the SnapshotSpawnCommand
    /// </summary>
    internal struct SnapshotSpawnDespawnCommandMeta
    {
        // The remaining clients a command still has to be sent to
        internal List<ulong> TargetClientIds;
    };

    /// <summary>
    /// Stores information about a specific client.
    /// What tick they ack'ed, for now.
    /// </summary>
    internal struct ClientData
    {
        internal int LastReceivedTick; // the last tick received by this client

        internal ClientData(int unused)
        {
            LastReceivedTick = -1;
        }
    }

    internal delegate int SendMessageHandler(SnapshotDataMessage message, ulong clientId);
    internal delegate void SpawnObjectHandler(SnapshotSpawnCommand spawnCommand, ulong srcClientId);
    internal delegate void DespawnObjectHandler(SnapshotDespawnCommand despawnCommand, ulong srcClientId);

    internal class SnapshotSystem : INetworkUpdateSystem, IDisposable
    {
        private NetworkManager m_NetworkManager;
        private NetworkTickSystem m_NetworkTickSystem;

        private Dictionary<ulong, ClientData> m_ClientData = new Dictionary<ulong, ClientData>();
        // todo: how is this unused and does it belong here ?
        private Dictionary<ulong, ConnectionRtt> m_ConnectionRtts = new Dictionary<ulong, ConnectionRtt>();

        // The tick we're currently processing (or last we processed, outside NetworkUpdate())
        private int m_CurrentTick = NetworkTickSystem.NoTick;

        // This arrays contains all the spawn commands received by the game code.
        // This part can be written as-is to the message.
        // Those are cleaned-up once the spawns are ack'ed by all target clients
        internal SnapshotSpawnCommand[] Spawns;
        // Meta-information about Spawns. Entries are matched by index
        internal SnapshotSpawnDespawnCommandMeta[] SpawnsMeta;
        // Number of spawns used in the array. The array might actually be bigger, as it reserves space for performance reasons
        internal int NumSpawns = 0;

        // This arrays contains all the despawn commands received by the game code.
        // This part can be written as-is to the message.
        // Those are cleaned-up once the despawns are ack'ed by all target clients
        internal SnapshotDespawnCommand[] Despawns;
        // Meta-information about Despawns. Entries are matched by index
        internal SnapshotSpawnDespawnCommandMeta[] DespawnsMeta;
        // Number of spawns used in the array. The array might actually be bigger, as it reserves space for performance reasons
        internal int NumDespawns = 0;

        // Local state. Stores which spawns and despawns were applied locally
        // indexed by ObjectId
        internal Dictionary<ulong, int> TickAppliedSpawn = new Dictionary<ulong, int>();
        internal Dictionary<ulong, int> TickAppliedDespawn = new Dictionary<ulong, int>();

        // Settings
        internal bool IsServer { get; set; }
        internal bool IsConnectedClient { get; set; }
        internal ulong ServerClientId { get; set; }
        internal List<ulong> ConnectedClientsId { get; } = new List<ulong>();

        internal SendMessageHandler SendMessage;
        internal SpawnObjectHandler SpawnObject;
        internal DespawnObjectHandler DespawnObject;

        // Property showing visibility into inner workings, for testing
        internal int SpawnsBufferCount { get; private set; } = 100;
        internal int DespawnsBufferCount { get; private set; } = 100;

        internal SnapshotSystem(NetworkManager networkManager, NetworkConfig config, NetworkTickSystem networkTickSystem)
        {
            m_NetworkManager = networkManager;
            m_NetworkTickSystem = networkTickSystem;

            if (networkManager != null)
            {
                // If we have a NetworkManager, let's send on the network. This can be overriden for tests
                SendMessage = NetworkSendMessage;
                // If we have a NetworkManager, let's (de)spawn with the rest of our package. This can be overriden for tests
                SpawnObject = NetworkSpawnObject;
                DespawnObject = NetworkDespawnObject;
            }

            // register for updates in EarlyUpdate
            this.RegisterNetworkUpdate(NetworkUpdateStage.EarlyUpdate);

            Spawns = new SnapshotSpawnCommand[SpawnsBufferCount];
            SpawnsMeta = new SnapshotSpawnDespawnCommandMeta[SpawnsBufferCount];
            Despawns = new SnapshotDespawnCommand[DespawnsBufferCount];
            DespawnsMeta = new SnapshotSpawnDespawnCommandMeta[DespawnsBufferCount];
        }

        // returns the default client list: just the server, on clients, all clients, on the server
        internal List<ulong> GetClientList()
        {
            List<ulong> clientList;
            clientList = new List<ulong>();

            if (!IsServer)
            {
                clientList.Add(m_NetworkManager.ServerClientId);
            }
            else
            {
                foreach (var clientId in ConnectedClientsId)
                {
                    if (clientId != m_NetworkManager.ServerClientId)
                    {
                        clientList.Add(clientId);
                    }
                }
            }

            return clientList;
        }

        /// <summary>
        /// Shrink the buffer to the minimum needed. Frees the reserved space.
        /// Mostly for testing at the moment, but could be useful for game code to reclaim memory
        /// </summary>
        internal void ReduceBufferUsage()
        {
            var count = Math.Max(1, NumDespawns);
            Array.Resize(ref Despawns, count);
            DespawnsBufferCount = count;

            count = Math.Max(1, NumSpawns);
            Array.Resize(ref Spawns, count);
            SpawnsBufferCount = count;
        }

        /// <summary>
        /// Called by SnapshotSystem, to spawn an object locally
        /// todo: consider observer pattern
        /// </summary>
        internal void NetworkSpawnObject(SnapshotSpawnCommand spawnCommand, ulong srcClientId)
        {
            NetworkObject networkObject;
            if (spawnCommand.ParentNetworkId == spawnCommand.NetworkObjectId)
            {
                networkObject = m_NetworkManager.SpawnManager.CreateLocalNetworkObject(false,
                    spawnCommand.GlobalObjectIdHash, spawnCommand.OwnerClientId, null, spawnCommand.ObjectPosition,
                    spawnCommand.ObjectRotation);
            }
            else
            {
                networkObject = m_NetworkManager.SpawnManager.CreateLocalNetworkObject(false,
                    spawnCommand.GlobalObjectIdHash, spawnCommand.OwnerClientId, spawnCommand.ParentNetworkId, spawnCommand.ObjectPosition,
                    spawnCommand.ObjectRotation);
            }

            m_NetworkManager.SpawnManager.SpawnNetworkObjectLocally(networkObject, spawnCommand.NetworkObjectId,
                true, spawnCommand.IsPlayerObject, spawnCommand.OwnerClientId, false);
            //todo: discuss with tools how to report shared bytes
            m_NetworkManager.NetworkMetrics.TrackObjectSpawnReceived(srcClientId, networkObject, 8);
        }

        /// <summary>
        /// Called by SnapshotSystem, to despawn an object locally
        /// </summary>
        internal void NetworkDespawnObject(SnapshotDespawnCommand despawnCommand, ulong srcClientId)
        {
            m_NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(despawnCommand.NetworkObjectId, out NetworkObject networkObject);

            m_NetworkManager.SpawnManager.OnDespawnObject(networkObject, true);
            //todo: discuss with tools how to report shared bytes
            m_NetworkManager.NetworkMetrics.TrackObjectDestroyReceived(srcClientId, networkObject, 8);
        }

        /// <summary>
        /// Updates the internal state of SnapshotSystem to refresh its knowledge of:
        /// - am I a server
        /// - what are the client Ids
        /// todo: consider optimizing
        /// </summary>
        internal void UpdateClientServerData()
        {
            if (m_NetworkManager)
            {
                IsServer = m_NetworkManager.IsServer;
                IsConnectedClient = m_NetworkManager.IsConnectedClient;
                ServerClientId = m_NetworkManager.ServerClientId;

                // todo: This is extremely inefficient. What is the efficient and idiomatic way ?
                ConnectedClientsId.Clear();
                if (IsServer)
                {
                    foreach (var id in m_NetworkManager.ConnectedClientsIds)
                    {
                        ConnectedClientsId.Add(id);
                    }
                }
            }
        }

        internal ConnectionRtt GetConnectionRtt(ulong clientId)
        {
            return new ConnectionRtt();
        }

        public void Dispose()
        {
            this.UnregisterNetworkUpdate(NetworkUpdateStage.EarlyUpdate);
        }

        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            if (updateStage == NetworkUpdateStage.EarlyUpdate)
            {
                UpdateClientServerData();

                var tick = m_NetworkTickSystem.LocalTime.Tick;

                if (tick != m_CurrentTick)
                {
                    m_CurrentTick = tick;
                    if (IsServer)
                    {
                        for (int i = 0; i < ConnectedClientsId.Count; i++)
                        {
                            var clientId = ConnectedClientsId[i];

                            // don't send to ourselves
                            if (clientId != ServerClientId)
                            {
                                SendSnapshot(clientId);
                            }
                        }
                    }
                    else if (IsConnectedClient)
                    {
                        SendSnapshot(ServerClientId);
                    }
                }
            }
        }

        // where we build and send a snapshot to a given client
        private void SendSnapshot(ulong clientId)
        {
            var header = new SnapshotHeader();
            var message = new SnapshotDataMessage(0);

            // Verify we allocated client Data for this clientId
            if (!m_ClientData.ContainsKey(clientId))
            {
                m_ClientData.Add(clientId, new ClientData(0));
            }

            // Find which spawns must be included
            var spawnsToInclude = new List<int>();
            for (var index = 0; index < NumSpawns; index++)
            {
                if (SpawnsMeta[index].TargetClientIds.Contains(clientId))
                {
                    spawnsToInclude.Add(index);
                }
            }

            // Find which despawns must be included
            var despawnsToInclude = new List<int>();
            for (var index = 0; index < NumDespawns; index++)
            {
                if (DespawnsMeta[index].TargetClientIds.Contains(clientId))
                {
                    despawnsToInclude.Add(index);
                }
            }

            header.CurrentTick = m_CurrentTick;
            header.SpawnCount = spawnsToInclude.Count;
            header.DespawnCount = despawnsToInclude.Count;
            header.LastReceivedSequence = m_ClientData[clientId].LastReceivedTick;

            if (!message.WriteBuffer.TryBeginWrite(
                FastBufferWriter.GetWriteSize(header) +
                spawnsToInclude.Count * FastBufferWriter.GetWriteSize(Spawns[0]) +
                despawnsToInclude.Count * FastBufferWriter.GetWriteSize(Despawns[0])))
            {
                // todo: error handling
                Debug.Assert(false, "Unable to secure buffer for sending");
            }

            message.WriteBuffer.WriteValue(header);

            // Write the Spawns.
            foreach (var index in spawnsToInclude)
            {
                message.WriteBuffer.WriteValue(Spawns[index]);
            }

            // Write the Despawns.
            foreach (var index in despawnsToInclude)
            {
                message.WriteBuffer.WriteValue(Despawns[index]);
            }

            SendMessage(message, clientId);
        }

        /// <summary>
        /// Entry-point into SnapshotSystem to spawn an object
        /// called with a SnapshotSpawnCommand, the NetworkObject and a list of target clientIds, where null means all clients
        /// </summary>
        internal void Spawn(SnapshotSpawnCommand command, NetworkObject networkObject, List<ulong> targetClientIds)
        {
            command.TickWritten = m_CurrentTick;

            if (NumSpawns >= SpawnsBufferCount)
            {
                SpawnsBufferCount = SpawnsBufferCount * 2;
                Array.Resize(ref Spawns, SpawnsBufferCount);
                Array.Resize(ref SpawnsMeta, SpawnsBufferCount);
            }

            if (targetClientIds == default)
            {
                targetClientIds = GetClientList();
            }

            // todo:
            // this 'if' might be temporary, but is needed to help in debugging
            // or maybe it stays
            if (targetClientIds.Count > 0)
            {
                Spawns[NumSpawns] = command;
                SpawnsMeta[NumSpawns].TargetClientIds = targetClientIds;
                NumSpawns++;
            }

            if (m_NetworkManager)
            {
                foreach (var dstClientId in targetClientIds)
                {
                    m_NetworkManager.NetworkMetrics.TrackObjectSpawnSent(dstClientId, networkObject, 8);
                }
            }
        }

        /// <summary>
        /// Entry-point into SnapshotSystem to despawn an object
        /// called with a SnapshotDespawnCommand, the NetworkObject and a list of target clientIds, where null means all clients
        /// </summary>
        internal void Despawn(SnapshotDespawnCommand command, NetworkObject networkObject, List<ulong> targetClientIds)
        {
            command.TickWritten = m_CurrentTick;

            if (NumDespawns >= DespawnsBufferCount)
            {
                DespawnsBufferCount = DespawnsBufferCount * 2;
                Array.Resize(ref Despawns, DespawnsBufferCount);
                Array.Resize(ref DespawnsMeta, DespawnsBufferCount);
            }

            if (targetClientIds == default)
            {
                targetClientIds = GetClientList();
            }

            // todo:
            // this 'if' might be temporary, but is needed to help in debugging
            // or maybe it stays
            if (targetClientIds.Count > 0)
            {
                Despawns[NumDespawns] = command;
                DespawnsMeta[NumDespawns].TargetClientIds = targetClientIds;
                NumDespawns++;
            }

            if (m_NetworkManager)
            {
                foreach (var dstClientId in targetClientIds)
                {
                    m_NetworkManager.NetworkMetrics.TrackObjectDestroySent(dstClientId, networkObject, 8);
                }
            }
        }

        // todo: entry-point for value updates
        internal void Store(ulong networkObjectId, int behaviourIndex, int variableIndex, NetworkVariableBase networkVariable)
        {
        }

        internal void HandleSnapshot(ulong clientId, SnapshotDataMessage message)
        {
            // Read the Spawns. Count first, then each spawn
            var spawnCommand = new SnapshotSpawnCommand();
            var despawnCommand = new SnapshotDespawnCommand();

            var header = new SnapshotHeader();

            // Verify we allocated client Data for this clientId
            if (!m_ClientData.ContainsKey(clientId))
            {
                m_ClientData.Add(clientId, new ClientData(0));
            }

            if (message.ReadBuffer.TryBeginRead(FastBufferWriter.GetWriteSize(header)))
            {
                // todo: error handling
                message.ReadBuffer.ReadValue(out header);
            }

            var clientData = m_ClientData[clientId];
            clientData.LastReceivedTick = header.CurrentTick;
            m_ClientData[clientId] = clientData;

            if (!message.ReadBuffer.TryBeginRead(FastBufferWriter.GetWriteSize(spawnCommand) * header.SpawnCount +
                                                 FastBufferWriter.GetWriteSize(despawnCommand) * header.DespawnCount))
            {
                // todo: deal with error
            }

            for (int index = 0; index < header.SpawnCount; index++)
            {
                message.ReadBuffer.ReadValue(out spawnCommand);

                if (TickAppliedSpawn.ContainsKey(spawnCommand.NetworkObjectId) &&
                    spawnCommand.TickWritten <= TickAppliedSpawn[spawnCommand.NetworkObjectId])
                {
                    continue;
                }

                TickAppliedSpawn[spawnCommand.NetworkObjectId] = spawnCommand.TickWritten;
                SpawnObject(spawnCommand, clientId);
            }

            for (int index = 0; index < header.DespawnCount; index++)
            {
                message.ReadBuffer.ReadValue(out despawnCommand);

                // todo: can we keep a single value of which tick we applied instead of per object ?
                if (TickAppliedDespawn.ContainsKey(despawnCommand.NetworkObjectId) &&
                    despawnCommand.TickWritten <= TickAppliedDespawn[despawnCommand.NetworkObjectId])
                {
                    continue;
                }

                TickAppliedDespawn[despawnCommand.NetworkObjectId] = despawnCommand.TickWritten;
                DespawnObject(despawnCommand, clientId);
            }

            for (int i = 0; i < NumSpawns;)
            {
                if (Spawns[i].TickWritten < header.LastReceivedSequence &&
                    SpawnsMeta[i].TargetClientIds.Contains(clientId))
                {
                    SpawnsMeta[i].TargetClientIds.Remove(clientId);

                    if (SpawnsMeta[i].TargetClientIds.Count == 0)
                    {
                        SpawnsMeta[i] = SpawnsMeta[NumSpawns - 1];
                        Spawns[i] = Spawns[NumSpawns - 1];
                        NumSpawns--;

                        continue; // skip the i++ below
                    }
                }
                i++;
            }
            for (int i = 0; i < NumDespawns;)
            {
                if (Despawns[i].TickWritten < header.LastReceivedSequence &&
                    DespawnsMeta[i].TargetClientIds.Contains(clientId))
                {
                    DespawnsMeta[i].TargetClientIds.Remove(clientId);

                    if (DespawnsMeta[i].TargetClientIds.Count == 0)
                    {
                        DespawnsMeta[i] = DespawnsMeta[NumDespawns - 1];
                        Despawns[i] = Despawns[NumDespawns - 1];
                        NumDespawns--;

                        continue; // skip the i++ below
                    }
                }
                i++;
            }


        }

        internal int NetworkSendMessage(SnapshotDataMessage message, ulong clientId)
        {
            m_NetworkManager.SendMessage(ref message, NetworkDelivery.ReliableFragmentedSequenced, clientId);

            return 0;
        }
    }
}
