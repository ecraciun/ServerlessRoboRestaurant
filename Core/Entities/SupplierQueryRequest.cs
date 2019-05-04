namespace Core.Entities
{
    public class SupplierQueryRequest
    {
        public string IngredientName { get; set; }
        public SupplierQueryStrategy QueryStrategy { get; set; }
    }
}
