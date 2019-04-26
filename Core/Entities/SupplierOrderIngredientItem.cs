using System.Diagnostics.CodeAnalysis;

namespace Core.Entities
{
    [ExcludeFromCodeCoverage]
    public class SupplierOrderIngredientItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}