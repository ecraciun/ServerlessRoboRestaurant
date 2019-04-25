﻿using System;
using System.Collections.Generic;

namespace Core.Entities
{
    public class SupplierOrder : EntityBase
    {
        public string SupplierId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public SupplierOrderStatus Status { get; set; }
        public List<SupplierOrderIngredientItem> OrderedItems { get; set; }
    }
}