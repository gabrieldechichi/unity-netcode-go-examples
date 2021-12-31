using UnityEngine;

namespace Core.Cooking
{
    [CreateAssetMenu(menuName = "Data/Cooking/IngredientTypeDefinitions")]
    public class IngredientTypeDefinitions : ScriptableObject
    {
        [System.Serializable]
        public struct IngredientTypeDefinition
        {
            public IngredientType Type;
            public Material Material;
        }

        [SerializeField]
        private IngredientTypeDefinition[] definitions;

        public IngredientTypeDefinition GetDefinitionForType(IngredientType type)
        {
            return System.Array.Find(definitions, d => d.Type == type);
        }
    }
}

