using Unity.Netcode;
using UnityEngine;

namespace Core.Cooking
{
    public class Ingredient : NetworkBehaviour
    {
        private IngredientTypeComponent IngredientTypeComponent => GetComponentInChildren<IngredientTypeComponent>();

        public IngredientType IngredientType
        {
            get => IngredientTypeComponent.IngredientType;
            set => IngredientTypeComponent.IngredientType = value;
        }
    }
}

