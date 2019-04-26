using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Core.Entities
{
    [ExcludeFromCodeCoverage]
    public class Recipe
    {
        public List<RecipeStep> Steps { get; set; }
        public int AproximateTotalTime => Steps.Sum(x => x.SecondsRequired);
    }
}