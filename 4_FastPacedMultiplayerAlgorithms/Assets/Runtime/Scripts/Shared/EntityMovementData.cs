using UnityEngine;

namespace Runtime.Simulation
{
    public class EntityMovementData : MonoBehaviour
    {
        public float MovementSpeed = 7;
        public float CollisionRadius = 0.5f;
        public LayerMask CollisionMask;
    }
}
