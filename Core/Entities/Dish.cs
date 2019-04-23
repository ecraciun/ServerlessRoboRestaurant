using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class Dish : TableEntity
    {
        public Dish()
        {
            this.PartitionKey = Constants.DefaultPartitionName;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public DishType Type { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public List<DishIngredient> Ingredients { get; set; }
        public Recipe Recipe { get; set; }
    }
}