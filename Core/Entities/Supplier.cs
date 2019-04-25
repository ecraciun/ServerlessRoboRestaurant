using System.Collections.Generic;

namespace Core.Entities
{
    public class Supplier : EntityBase
    {
        public string Name { get; set; }
        public int TimeToDelivery { get; set; }
        public List<SupplierIngredient> IngredientsForSale { get; set; }
    }
}