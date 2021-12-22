using Unity.Netcode;
using UnityEngine;

namespace ClientDriven.Common
{
    /// <summary>
    /// NetworkBehaviour that is enable only in specific machines (ex: Server or Client)
    /// See ClientBehaviour and ServerBeaviour
    /// </summary>
    public abstract class ExclusiveNeworkBehaviour : NetworkBehaviour
    {
        protected abstract bool ShouldBeActive();
        public override sealed void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (ShouldBeActive())
            {
                OnNetworkSpawnInternal();
            }
            else
            {
                enabled = false;
            }
        }

        public override sealed void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (ShouldBeActive())
            {
                OnNetworkDespawnInternal();
            }
        }

        public override sealed void OnGainedOwnership()
        {
            base.OnGainedOwnership();
            if (ShouldBeActive())
            {
                OnGainedOwnershipInternal();
            }
        }


        public override sealed void OnLostOwnership()
        {
            base.OnLostOwnership();
            if (ShouldBeActive())
            {
                OnLostOwnershipInternal();
            }
        }


        public override sealed void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
        {
            base.OnNetworkObjectParentChanged(parentNetworkObject);
            if (ShouldBeActive())
            {
                OnNetworkObjectParentChangedInternal(parentNetworkObject);
            }
        }

        protected virtual void OnGainedOwnershipInternal()
        {
        }
        protected virtual void OnLostOwnershipInternal()
        {
        }
        protected virtual void OnNetworkObjectParentChangedInternal(NetworkObject parentNetworkObject)
        {
        }

        protected virtual void OnNetworkDespawnInternal()
        {
        }

        protected virtual void OnNetworkSpawnInternal()
        {
        }

    }

}
