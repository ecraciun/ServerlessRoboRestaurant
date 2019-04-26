using System.Diagnostics.CodeAnalysis;

namespace Core.Entities
{
    [ExcludeFromCodeCoverage]
    public class RecipeStep
    {
        public int StepOrder { get; set; }
        public int SecondsRequired { get; set; }
        public string StepName { get; set; }
    }
}