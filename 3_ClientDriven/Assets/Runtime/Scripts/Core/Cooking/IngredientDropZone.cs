namespace Core.Cooking
{
    public class IngredientDropZone : IngredientTypeTrigger
    {
        protected override void OnIngredientEnterTrigger(Ingredient ingredient)
        {
            if (ingredient.IngredientType == IngredientTypeComponent.IngredientType)
            {
                ingredient.NetworkObject.Despawn();
            }
        }
    }
}

