namespace Core.Cooking
{
    public class IngredientStove : IngredientTypeTrigger
    {
        protected override void OnIngredientEnterTrigger(Ingredient ingredient)
        {
            if (ingredient.IngredientType != IngredientTypeComponent.IngredientType)
            {
                ingredient.IngredientType = IngredientTypeComponent.IngredientType;
            }
        }
    }
}

