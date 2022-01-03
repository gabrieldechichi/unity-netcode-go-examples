using UnityEngine;

namespace Runtime.Simulation
{

    public class NetworkBehaviour : MonoBehaviour
    {
        public NetworkEntity Entity => GetComponent<NetworkEntity>();
        public string EntityId => Entity.EntityId;

        public virtual void OnNetworkSpawn()
        {
        }
    }
}
