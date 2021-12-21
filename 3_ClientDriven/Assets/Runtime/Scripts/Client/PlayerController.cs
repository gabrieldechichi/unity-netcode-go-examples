using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ClientDriven.Client
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float speed = 10;
        [SerializeField] private float rotateSpeed = 10;
        InputActions input;
        CharacterController charCtrl;

        private void Awake()
        {

        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsClient)
            {
                Destroy(this);
            }
            else
            {
                charCtrl = GetComponent<CharacterController>();
                input = new InputActions();
                input.PlayerControls.Enable();
                //TODO (hack): Hardcoded spawn position
                transform.position = new Vector3(-48.7f, 0.9f, -11.6f);
            }
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
    }
}