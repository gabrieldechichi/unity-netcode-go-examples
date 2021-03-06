using UnityEngine;

namespace Runtime.Simulation
{
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
