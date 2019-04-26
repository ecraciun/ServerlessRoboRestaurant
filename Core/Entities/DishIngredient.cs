using System.Diagnostics.CodeAnalysis;

namespace Core.Entities
{
    [ExcludeFromCodeCoverage]
    public class DishIngredient
    {
        public string Name { get; set; }
        public string StockIngredientId { get; set; }
        public int QuantityNeeded { get; set; }
    }
}