using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Core.Entities
{
    [ExcludeFromCodeCoverage]
    public class Order : EntityBase
    {
        public DateTime TimePlacedUtc { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime LastModifiedUtc { get; set; }
    }
}