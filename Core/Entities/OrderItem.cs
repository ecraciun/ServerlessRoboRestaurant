using System.Diagnostics.CodeAnalysis;

namespace Core.Entities
{
    [ExcludeFromCodeCoverage]
    public class OrderItem
    {
        public string DishId { get; set; }
        public int Quantity { get; set; }
    }
}