using Core.Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Core.Cooking
{
    public class SpawnIngredientInteractable : SpawnObjectInteractable
    {
        [SerializeField] private float throwForce = 10;
        protected override void OnNewInstanceSpawned(NetworkObject instance)
        {
            base.OnNewInstanceSpawned(instance);

            if (instance.TryGetComponent<Rigidbody>(out var rb)
                && !rb.isKinematic)
            {
                var dir = Random.insideUnitSphere;
                dir.y = 2 * Mathf.Abs(dir.y);
                rb.AddForce(dir * throwForce, ForceMode.Impulse);
            }

            if (instance.TryGetComponent<Ingredient>(out var ingredient))
            {
                var ingredientType = (IngredientType)Random.Range(0, (int)IngredientType.Max);
                ingredient.IngredientType = ingredientType;
            }
        }
    }
}

