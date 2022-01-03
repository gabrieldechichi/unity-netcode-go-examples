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

    public abstract class ExclusiveNetworkBehaviour : NetworkBehaviour
    {
        protected abstract EntityNetworkRole EnabledRole { get; }

        public override sealed void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log($"{Entity.Role}, {EnabledRole}");
            if (Entity.Role != EnabledRole)
            {
                Destroy(this);
            }
            else
            {
                OnNetworkSpawnInternal();
            }
        }

        protected virtual void OnNetworkSpawnInternal()
        {
        }
    }
}
