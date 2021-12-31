using Unity.Netcode;
using UnityEngine;

namespace Core.Cooking
{
    [RequireComponent(typeof(Collider))]
    public class IngredientDropZone : NetworkBehaviour
    {
        private IngredientTypeComponent IngredientTypeComponent => GetComponentInChildren<IngredientTypeComponent>();

        private Collider Collider => GetComponent<Collider>();
        private void Awake()
        {
            Collider.enabled = false;
            Collider.isTrigger = true;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            var isServer = IsServer;
            enabled = isServer;
            Collider.enabled = isServer;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Ingredient>(out var ingredient)
                && ingredient.IngredientType == IngredientTypeComponent.IngredientType)
            {
                ingredient.NetworkObject.Despawn();
            }
        }
    }
}

