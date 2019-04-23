using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Core.Entities
{
    public class Order : TableEntity
    {
        public Order()
        {
            this.PartitionKey = Constants.DefaultPartitionName;
        }

        public Order(string orderNumber) : base()
        {
            this.RowKey = orderNumber;
        }

        public DateTime TimePlaced { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public OrderStatus Status { get; set; }
    }
}