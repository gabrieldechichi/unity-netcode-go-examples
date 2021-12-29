using System;
using Core.Interaction;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Player
{
    [RequireComponent(
        typeof(CharacterController),
        typeof(ClientNetworkTransform),
        typeof(PlayerInitializer))]
    [RequireComponent(
        typeof(InteractionComponent)
    )]
    public partial class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float speed = 10;
        [SerializeField] private float rotateSpeed = 10;
        private CharacterController charCtrl;
        private InputActions inputActions;

        private InteractionComponent interactionComp;

        private void Awake()
        {
            charCtrl = GetComponent<CharacterController>();
            interactionComp = GetComponent<InteractionComponent>();
            inputActions = new InputActions();
            inputActions.PlayerControls.Enable();

            inputActions.PlayerControls.Interact.performed += OnInteractPressed;
        }

        private void OnEnable()
        {
            if (IsServer && !IsHost)
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

        private void OnInteractPressed(InputAction.CallbackContext obj)
        {
            interactionComp.TryInteract();
        }
    }
}

