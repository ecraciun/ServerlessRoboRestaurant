using System.Diagnostics.CodeAnalysis;

namespace Core.Entities
{
    [ExcludeFromCodeCoverage]
    public class StockIngredient : EntityBase
    {
        public string Name { get; set; }
        public int StockQuantity { get; set; }
    }
}