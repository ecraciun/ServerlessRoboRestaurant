using Core.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using TestsCommon;
using Xunit;

namespace Restaurant.Tests
{
    public class RecipeStepActivityTests
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task Run_Should_Take_At_Least_X_Seconds(int seconds)
        {
            var step = new RecipeStep
            {
                SecondsRequired = seconds,
                StepName = $"Test {seconds}",
                StepOrder = seconds
            };

            var sw = new Stopwatch();
            sw.Start();
            await RecipeStepActivity.Run(step, _logger);
            sw.Stop();

            Assert.True(seconds < sw.Elapsed.TotalSeconds);
        }
    }
}