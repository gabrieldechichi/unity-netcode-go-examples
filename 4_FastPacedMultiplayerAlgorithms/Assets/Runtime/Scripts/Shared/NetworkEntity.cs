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

        public void OnNetworkSpawn()
        {
            foreach (var comp in GetComponentsInChildren<NetworkBehaviour>())
            {
                comp.OnNetworkSpawn();
            }
        }
    }
}
