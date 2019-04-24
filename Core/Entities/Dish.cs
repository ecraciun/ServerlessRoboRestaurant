using System.Collections.Generic;

namespace Core.Entities
{
    public class Dish : EntityBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DishType Type { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public List<DishIngredient> Ingredients { get; set; }
        public Recipe Recipe { get; set; }
    }
}