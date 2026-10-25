namespace NomadChef.Orders
{
    /// <summary>One ticket in the order queue, counting down until the customer gives up.</summary>
    public class Order
    {
        public Order(DishRecipeSO recipe)
        {
            Recipe = recipe;
            TimeLeft = recipe.Patience;
        }

        public DishRecipeSO Recipe { get; }
        public float TimeLeft { get; internal set; }
        public float NormalizedTimeLeft => TimeLeft / Recipe.Patience;
    }
}
