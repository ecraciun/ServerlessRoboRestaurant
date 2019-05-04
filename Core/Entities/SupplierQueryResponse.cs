namespace Core.Entities
{
    public class SupplierQueryResponse
    {
        public string SupplierId { get; set; }
        public decimal UnitPrice { get; set; }
        public int TimeToDelivery { get; set; }
    }
}