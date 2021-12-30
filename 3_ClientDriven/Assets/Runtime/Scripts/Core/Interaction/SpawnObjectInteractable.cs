using Unity.Netcode;
using UnityEngine;

namespace Core.Interaction
{
    public class SpawnObjectInteractable : InteractableBase
    {
        [SerializeField] private Vector3 localSpawnPosition;
        [SerializeField] private float throwForce = 20;
        [SerializeField] private NetworkObject[] spawnPrefabs;


        private Vector3 SpawnPosition => transform.TransformPoint(localSpawnPosition);

        protected override void EndInteraction_Internal(InteractionComponent interactionComponent)
        {
            if (spawnPrefabs.Length > 0)
            {
                var prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];
                var instance = Instantiate(prefab, SpawnPosition, Quaternion.identity);
                instance.Spawn();

                if (instance.TryGetComponent<Rigidbody>(out var rb)
                    && !rb.isKinematic)
                {
                    var dir = Random.insideUnitSphere;
                    dir.y = 2 * Mathf.Abs(dir.y);
                    rb.AddForce(dir * throwForce, ForceMode.Impulse);
                }
            }
        }

        protected override void StartInteraction_Internal(InteractionComponent interactionComponent)
        {
            //Will call EndInteraction
            interactionComponent.EndInteraction();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(SpawnPosition, 1.0f);
        }
    }
}

