using Unity.Netcode.Components;
using UnityEngine;

namespace Core.Interaction
{
    [RequireComponent(
        typeof(Rigidbody),
        typeof(NetworkRigidbody),
        typeof(NetworkTransform))]
    public class PickThrowInteractable : InteractableBase
    {
        [SerializeField] private Vector3 relativeThrowDirection = new Vector3(0, 1, 2);
        [SerializeField] private float throwForce = 10;

        private Rigidbody rb;
        private NetworkTransform networkTransform;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            networkTransform = GetComponent<NetworkTransform>();
        }

        protected override void StartInteraction_Internal(InteractionComponent interactionComponent)
        {
            rb.isKinematic = true;
            transform.SetParent(interactionComponent.transform);
            networkTransform.InLocalSpace = true;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.up * 1.5f;
        }

        protected override void EndInteraction_Internal(InteractionComponent interactionComponent)
        {
            //Order is important here
            var forceVector = GetThrowForceVector();

            transform.SetParent(null);
            rb.isKinematic = false;
            networkTransform.InLocalSpace = false;

            rb.AddForce(forceVector, ForceMode.Impulse);
        }

        private Vector3 GetThrowForceVector()
        {
            var throwDir = relativeThrowDirection;
            if (transform.parent != null)
            {
                throwDir = transform.parent.TransformDirection(relativeThrowDirection);
            }
            var forceVector = throwDir.normalized * throwForce;
            return forceVector;
        }

        private void OnDrawGizmos()
        {
            var forceVector = GetThrowForceVector();
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, forceVector);
        }
    }
}

