using Core.Camera;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    public class PlayerInitializer : NetworkBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private ThirdPersonCamera cam;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            cam.transform.SetParent(null);
            if (IsServer || !IsOwner)
            {
                cam.GetComponent<UnityEngine.Camera>().enabled = false;
            }
        }
    }
}

