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
            if (!IsClient)
            {
                Destroy(this);
            }
            else
            {
                charCtrl = GetComponent<CharacterController>();
                input.PlayerControls.Enable();
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
            charCtrl.SimpleMove(movement * speed * Time.deltaTime);

            var mouseDelta = input.PlayerControls.MouseDelta.ReadValue<Vector2>();
            var rotation = mouseDelta.x * rotateSpeed;
            transform.Rotate(0, rotation * Time.deltaTime, 0);
        }
    }
}