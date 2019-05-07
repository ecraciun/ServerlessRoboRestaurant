using Core;
using Core.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestsCommon;
using Xunit;

namespace Restaurant.Tests
{
    public class DishOrchestratorTests
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();
        private readonly Mock<DurableOrchestrationContextBase> _contextMock;
        private Dish TestDish
        {
            get
            {
                return new Dish
                {
                    Description = "test",
                    ETag = "1",
                    Id = "1",
                    ImageUrl = "1",
                    Ingredients = new List<DishIngredient>(),
                    Price = 10,
                    Title = "Test",
                    Type = DishType.Main,
                    Recipe = new Recipe
                    {
                        Steps = new List<RecipeStep>
                        {
                            new RecipeStep
                            {
                                SecondsRequired = 1,
                                StepName = "Step 1",
                                StepOrder = 1
                            },
                            new RecipeStep
                            {
                                SecondsRequired = 1,
                                StepName = "Step 2",
                                StepOrder = 2
                            },
                            new RecipeStep
                            {
                                SecondsRequired = 1,
                                StepName = "Step 3",
                                StepOrder = 3
                            }
                        }
                    }
                };
            }
        }

        public DishOrchestratorTests()
        {
            _contextMock = new Mock<DurableOrchestrationContextBase>();
            _contextMock.Setup(
                x => x.CallActivityAsync(Constants.RecipeStepActivityFunctionName, It.IsAny<RecipeStep>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_Dish_Is_Null()
        {
            _contextMock.Setup(x => x.GetInput<Dish>()).Returns<Dish>(null);
            var result = await DishOrchestrator.RunOrchestrator(_contextMock.Object);

            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_Recipe_Is_Null()
        {
            var tmp = TestDish;
            tmp.Recipe = null;
            _contextMock.Setup(x => x.GetInput<Dish>()).Returns(tmp);
            var result = await DishOrchestrator.RunOrchestrator(_contextMock.Object);

            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_RecipeSteps_Is_Null()
        {
            var tmp = TestDish;
            tmp.Recipe.Steps = null;
            _contextMock.Setup(x => x.GetInput<Dish>()).Returns(tmp);
            var result = await DishOrchestrator.RunOrchestrator(_contextMock.Object);

            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_RecipeSteps_Is_Empty()
        {
            var tmp = TestDish;
            tmp.Recipe.Steps = new List<RecipeStep>();
            _contextMock.Setup(x => x.GetInput<Dish>()).Returns(tmp);
            var result = await DishOrchestrator.RunOrchestrator(_contextMock.Object);

            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_True_When_Dish_Is_Ok()
        {
            _contextMock.Setup(x => x.GetInput<Dish>()).Returns(TestDish);
            var result = await DishOrchestrator.RunOrchestrator(_contextMock.Object);

            Assert.True(result);
            _contextMock.Verify(
                x => x.CallActivityAsync(Constants.RecipeStepActivityFunctionName, It.IsAny<RecipeStep>()),
                    Times.Exactly(TestDish.Recipe.Steps.Count));
        }
    }
}