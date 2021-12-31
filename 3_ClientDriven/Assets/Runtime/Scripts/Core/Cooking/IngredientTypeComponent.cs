using System;
using Unity.Netcode;
using UnityEngine;

namespace Core.Cooking
{
    public enum IngredientType
    {
        Red, Blue, Purple, Max
    }

    [RequireComponent(typeof(Renderer))]
    public class IngredientTypeComponent : NetworkBehaviour
    {
        [SerializeField] private IngredientTypeDefinitions definitions;
        [SerializeField] private NetworkVariable<IngredientType> ingredientType = new NetworkVariable<IngredientType>();

        private Renderer Renderer => GetComponent<Renderer>();

        public IngredientType IngredientType
        {
            get => ingredientType.Value;
            set
            {
                ingredientType.Value = value;
                UpdateMaterial();
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ingredientType.OnValueChanged += OnIngredientTypeChanged;
            UpdateMaterial();
        }

        private void OnIngredientTypeChanged(IngredientType previousValue, IngredientType newValue)
        {
            UpdateMaterial();
        }

        private void UpdateMaterial()
        {
            if (definitions != null)
            {
                Renderer.material = definitions.GetDefinitionForType(IngredientType).Material;
            }
        }

        private void OnValidate()
        {
            UpdateMaterial();
        }
    }
}

