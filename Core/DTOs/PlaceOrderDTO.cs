using Core.Entities;
using System.Collections.Generic;

namespace Core.DTOs
{
    public class PlaceOrderDTO
    {
        public List<OrderItem> OrderItems { get; set; }
    }
}