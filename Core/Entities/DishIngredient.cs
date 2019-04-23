namespace Core.Entities
{
    public class DishIngredient
    {
        public string Name { get; set; }
        public string StockIngredientId { get; set; }
        public int QuantityNeeded { get; set; }
    }
}