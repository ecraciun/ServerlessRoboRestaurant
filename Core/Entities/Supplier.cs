using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Core.Entities
{
    [ExcludeFromCodeCoverage]
    public class Supplier : EntityBase
    {
        public string Name { get; set; }
        public int TimeToDelivery { get; set; }
        public List<SupplierIngredient> IngredientsForSale { get; set; }
    }
}