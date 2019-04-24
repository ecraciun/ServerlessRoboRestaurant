using System;
using System.Collections.Generic;

namespace Core.Entities
{
    public class Order : EntityBase
    {
        public DateTime TimePlaced { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public OrderStatus Status { get; set; }
    }
}