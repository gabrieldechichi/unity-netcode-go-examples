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

        [SerializeField] private float collisionRadius = 10;

        [SerializeField] private LayerMask collisionMask = 0;

        public EntityNetworkRole Role { get; internal set; }

        private RaycastHit2D[] hits = new RaycastHit2D[5];

        internal void Server_ProcessMovementInput(MovementInput msg, float dt)
        {
            if (IsInputValid(msg))
            {
                var position = transform.position;
                var frameMovement = (movementSpeed * msg.FrameInputX);
                var prevPos = position;
                position.x += frameMovement;

                CheckCollisions(ref position, prevPos, frameMovement);

                transform.position = position;
            }
        }

        private bool IsInputValid(MovementInput msg)
        {
            //TODO: Better validation.
            const float maxClientDt = 1.0f / 20.0f;
            return Mathf.Abs(msg.FrameInputX) <= maxClientDt;
        }

        private void CheckCollisions(ref Vector3 currentPosition, Vector3 previousPosition, float frameMovement)
        {
            var ray = Vector2.right * frameMovement;
            var hitCount = Physics2D.CircleCastNonAlloc(previousPosition, collisionRadius, ray.normalized, hits, Mathf.Abs(frameMovement), collisionMask);
            for (int i = 0; i < hitCount; i++)
            {
                var hit = hits[i];
                currentPosition = hit.point + hit.normal * collisionRadius;
            }
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

        internal MovementInput Client_ProcessInput(float dt)
        {
            var movementInput = Input.GetAxisRaw("Horizontal");
            return new MovementInput
            {
                EntityId = EntityId,
                FrameInputX = movementInput * dt
            };
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, collisionRadius);
        }
    }
}
