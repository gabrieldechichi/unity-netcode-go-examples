using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;

namespace Core.Player
{
    [RequireComponent(
        typeof(CharacterController),
        typeof(ClientNetworkTransform),
        typeof(PlayerInitializer))]
    public class PlayerController : NetworkBehaviour
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
                //TODO (hack): Hardcoding spawn position
                {
                    charCtrl.enabled = false;
                    transform.position = new Vector3(-45.76f, 1f, -10.65f);
                    charCtrl.enabled = true;
                }
            }
        }

        private void FixedUpdate()
        {
            if (IsOwner && IsClient)
            {
                var rotInput = inputActions.PlayerControls.MouseDelta.ReadValue<Vector2>();
                transform.Rotate(0, rotInput.x * rotateSpeed * Time.fixedDeltaTime, 0);

                var moveinput = inputActions.PlayerControls.Move.ReadValue<Vector2>();
                var ds = (transform.forward * moveinput.y + transform.right * moveinput.x) * speed;
                charCtrl.SimpleMove(ds);
            }
        }
    }
}

