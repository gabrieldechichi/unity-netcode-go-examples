using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;

namespace Core.Player
{
    [RequireComponent(
        typeof(CharacterController),
        typeof(ClientNetworkTransform),
        typeof(PlayerInitializer))]
    public partial class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float speed = 10;
        [SerializeField] private float rotateSpeed = 10;
        private CharacterController charCtrl;
        private InputActions inputActions;

        private void Awake()
        {
            charCtrl = GetComponent<CharacterController>();
            inputActions = new InputActions();
            inputActions.PlayerControls.Enable();
        }

        private void OnEnable()
        {
            if (IsServer)
            {
                enabled = false;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner && IsClient)
            {
                TEMP_ClientSetSpawnPosition();
            }
        }

        private void FixedUpdate()
        {
            if (IsOwner && IsClient)
            {
                ClientProcessInput();
            }
        }
    }
}

