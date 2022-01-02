using UnityEngine;

namespace Runtime.Simulation
{
    public class EntityMovementData : MonoBehaviour
    {
        [SerializeField] private float movementSpeed = 10;

        [SerializeField] private float collisionRadius = 10;

        [SerializeField] private LayerMask collisionMask = 0;

        public float MovementSpeed => movementSpeed;
        public float CollisionRadius => collisionRadius;
        public LayerMask CollisionMask => collisionMask;
    }
}
