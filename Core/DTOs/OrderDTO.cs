﻿using Core.Entities;

namespace Core.DTOs
{
    public class OrderDTO
    {
        public string OrderId { get; set; }
        public string OrchestratorId { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}