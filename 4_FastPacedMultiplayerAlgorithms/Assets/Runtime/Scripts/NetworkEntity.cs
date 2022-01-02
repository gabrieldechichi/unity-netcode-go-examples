using System;
using UnityEngine;

namespace Runtime.Simulation
{
    public enum EntityNetworkRole
    {
        Server, OwningClient, Ghost
    }

    public class NetworkEntity : MonoBehaviour
    {
        [HideInInspector] public string EntityId;

        public EntityNetworkRole Role { get; internal set; }

        internal void Server_ProcessMovementInput(MovementInput msg)
        {
            Debug.Log("Server Process Movement Input");
        }

        internal EntitySnapshot Server_GenerateSnapshot()
        {
            Debug.Log("Server Generate Snapshot");
            return new EntitySnapshot();
        }

        internal void Client_ReceiveServerSnapshot(EntitySnapshot snapshot)
        {
            Debug.Log("Client Receive Server Snapshot");
        }

        internal MovementInput Client_ProcessInput()
        {
            Debug.Log("Client PRocess Input");
            return new MovementInput();
        }
    }
}
