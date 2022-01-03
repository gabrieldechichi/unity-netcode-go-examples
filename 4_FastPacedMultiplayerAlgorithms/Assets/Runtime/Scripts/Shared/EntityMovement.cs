using UnityEngine;

namespace Runtime.Simulation
{
    [RequireComponent(typeof(EntityMovementData), typeof(NetworkEntity))]
    public abstract class EntityMovement : ExclusiveNetworkBehaviour
    {
        private RaycastHit2D[] hits = new RaycastHit2D[5];

        private EntityMovementData MovementData => GetComponent<EntityMovementData>();

        protected float MovementSpeed => MovementData.MovementSpeed;
        protected float CollisionRadius => MovementData.CollisionRadius;
        protected LayerMask CollisionMask => MovementData.CollisionMask;


        public abstract void Move(MovementInput msg);

        protected void ExecuteMove(MovementInput msg)
        {
            var position = transform.position;
            var frameMovement = (MovementSpeed * msg.FrameInputX);
            var prevPos = position;
            position.x += frameMovement;

            CheckCollisions(ref position, prevPos, frameMovement);

            transform.position = position;
        }

        private void CheckCollisions(ref Vector3 currentPosition, Vector3 previousPosition, float frameMovement)
        {
            var ray = Vector2.right * frameMovement;
            var hitCount = Physics2D.CircleCastNonAlloc(previousPosition, CollisionRadius, ray.normalized, hits, Mathf.Abs(frameMovement), CollisionMask);
            for (int i = 0; i < hitCount; i++)
            {
                var hit = hits[i];
                currentPosition = hit.point + hit.normal * CollisionRadius;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, CollisionRadius);
        }
    }
}
