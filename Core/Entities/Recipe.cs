using System.Collections.Generic;
using System.Linq;

namespace Core.Entities
{
    public class Recipe
    {
        public List<RecipeStep> Steps { get; set; }
        public int AproximateTotalTime => Steps.Sum(x => x.SecondsRequired);
    }
}