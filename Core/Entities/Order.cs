using System;
using System.Collections.Generic;

namespace Core.Entities
{
    public class Order : EntityBase
    {
        public DateTime TimePlacedUtc { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime LastModifiedUtc { get; set; }
    }
}