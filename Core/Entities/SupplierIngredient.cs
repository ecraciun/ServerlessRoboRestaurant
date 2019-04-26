using System.Diagnostics.CodeAnalysis;

namespace Core.Entities
{
    [ExcludeFromCodeCoverage]
    public class SupplierIngredient
    {
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
    }
}