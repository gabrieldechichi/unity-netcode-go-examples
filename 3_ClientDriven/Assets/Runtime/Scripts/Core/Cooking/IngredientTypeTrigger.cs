using Unity.Netcode;
using UnityEngine;

namespace Core.Cooking
{
    [RequireComponent(typeof(Collider))]
    public abstract class IngredientTypeTrigger : NetworkBehaviour
    {
        protected IngredientTypeComponent IngredientTypeComponent => GetComponentInChildren<IngredientTypeComponent>();

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
            if (other.TryGetComponent<Ingredient>(out var ingredient))
            {
                OnIngredientEnterTrigger(ingredient);
            }
        }

        protected abstract void OnIngredientEnterTrigger(Ingredient ingredient);
    }
}

