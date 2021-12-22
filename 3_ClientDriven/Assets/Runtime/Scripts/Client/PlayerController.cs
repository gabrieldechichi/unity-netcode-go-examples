using ClientDriven.Common;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace ClientDriven.Client
{
    [RequireComponent(
        typeof(CharacterController),
        typeof(ClientInteractionComponent))]
    public class PlayerController : ClientBehaviour
    {
        [SerializeField] private float speed = 10;
        [SerializeField] private float rotateSpeed = 10;
        InputActions input;
        CharacterController charCtrl;
        public ClientInteractionComponent ClientInteraction { get; private set; }

        private void OnEnable()
        {
            if (input == null)
            {
                input = new InputActions();
                input.Enable();
            }

            input.PlayerControls.Interact.performed += OnInteractPressed;
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.PlayerControls.Interact.performed -= OnInteractPressed;
            }
        }


        protected override void OnNetworkSpawnInternal()
        {
            base.OnNetworkSpawnInternal();
            charCtrl = GetComponent<CharacterController>();
            ClientInteraction = GetComponent<ClientInteractionComponent>();
            //TODO (hack): Hardcoded spawn position
            transform.position = new Vector3(-48.7f, 0.9f, -11.6f);
        }

        private void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            var movInput = input.PlayerControls.Move.ReadValue<Vector2>();
            var movement = transform.forward * movInput.y + transform.right * movInput.x;
            charCtrl.SimpleMove(movement * speed);

            var mouseDelta = input.PlayerControls.MouseDelta.ReadValue<Vector2>();
            var rotation = mouseDelta.x * rotateSpeed;
            transform.Rotate(0, rotation * Time.deltaTime, 0);

        }

        private void OnInteractPressed(InputAction.CallbackContext obj)
        {
            Assert.IsNotNull(ClientInteraction);
            if (ClientInteraction != null)
            {
                ClientInteraction.TryInteract();
            }
        }
    }
}