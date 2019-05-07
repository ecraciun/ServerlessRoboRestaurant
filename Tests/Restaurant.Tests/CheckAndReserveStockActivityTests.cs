using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using TestsCommon;
using Xunit;

namespace Restaurant.Tests
{
    public class CheckAndReserveStockActivityTests
    {
        private readonly Mock<IBaseRepositoryFactory<StockIngredient>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<StockIngredient>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();
        private const string TestIngredientName = "test";

        public CheckAndReserveStockActivityTests()
        {
            _repoMock = new Mock<IBaseRepository<StockIngredient>>();
            _repoMock.Setup(x => x.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new StockIngredient { Id = "test", StockQuantity = 6 }));
            _repoMock.Setup(x => x.TryUpdateWithRetry(It.IsAny<StockIngredient>(), It.IsAny<Action<StockIngredient>>(), It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<StockIngredient>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        [Fact]
        public async Task Run_Should_Return_False_And_Empty_When_Ingredient_Is_Null()
        {
            var result = await CheckAndReserveStockActivity.Run(
                null, _repoFactoryMock.Object, _logger);

            Assert.False(result.Reserved);
            Assert.True(string.IsNullOrEmpty(result.IngredientName));
        }

        [Fact]
        public async Task Run_Should_Return_False_And_Empty_When_IngredientId_Is_Null()
        {
            var result = await CheckAndReserveStockActivity.Run(
                new DishIngredient
                {
                    Name = TestIngredientName,
                    QuantityNeeded = 1,
                    StockIngredientId = null
                }, _repoFactoryMock.Object, _logger);

            Assert.False(result.Reserved);
            Assert.True(string.IsNullOrEmpty(result.IngredientName));
        }

        [Fact]
        public async Task Run_Should_Return_False_And_Empty_When_Quantity_Is_Le_0()
        {
            var result = await CheckAndReserveStockActivity.Run(
                new DishIngredient
                {
                    StockIngredientId = "abc",
                    QuantityNeeded = -1,
                    Name = TestIngredientName
                }, _repoFactoryMock.Object, _logger);

            Assert.False(result.Reserved);
            Assert.True(string.IsNullOrEmpty(result.IngredientName));
        }

        [Fact]
        public async Task Run_Should_Return_False_And_IngredientName_When_Quantity_Is_Gt_Available_Stock()
        {
            var result = await CheckAndReserveStockActivity.Run(
                new DishIngredient
                {
                    StockIngredientId = "abc",
                    QuantityNeeded = 100,
                    Name = TestIngredientName
                }, _repoFactoryMock.Object, _logger);

            Assert.False(result.Reserved);
            Assert.Equal(TestIngredientName, result.IngredientName);
        }

        [Fact]
        public async Task Run_Should_Return_True_And_Empty_When_All_Is_Ok()
        {
            var result = await CheckAndReserveStockActivity.Run(
                new DishIngredient
                {
                    StockIngredientId = "abc",
                    QuantityNeeded = 2,
                    Name = TestIngredientName
                }, _repoFactoryMock.Object, _logger);

            Assert.True(result.Reserved);
            Assert.True(string.IsNullOrEmpty(result.IngredientName));
        }
    }
}