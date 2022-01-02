using System;
using UnityEngine;

namespace Runtime.Simulation
{
    public enum EntityNetworkRole
    {
        Server, OwningClient, Ghost
    }

    public class NetworkEntity : MonoBehaviour
    {
        [HideInInspector] public string EntityId;
        [SerializeField] private float movementSpeed = 10;

        public EntityNetworkRole Role { get; internal set; }

        internal void Server_ProcessMovementInput(MovementInput msg, float dt)
        {
            var moveInput = Mathf.Clamp(msg.InputX, -1, 1);
            var position = transform.position;
            position.x += (dt * movementSpeed * moveInput);

            transform.position = position;
        }

        internal EntitySnapshot Server_GenerateSnapshot()
        {
            return new EntitySnapshot
            {
                EntityId = EntityId,
                PositionX = transform.position.x
            };
        }

        internal void Client_ReceiveServerSnapshot(EntitySnapshot snapshot)
        {
            var position = transform.position;
            position.x = snapshot.PositionX;
            transform.position = position;
        }

        internal MovementInput Client_ProcessInput()
        {
            var movementInput = Input.GetAxisRaw("Horizontal");
            return new MovementInput
            {
                EntityId = EntityId,
                InputX = movementInput
            };
        }
    }
}
