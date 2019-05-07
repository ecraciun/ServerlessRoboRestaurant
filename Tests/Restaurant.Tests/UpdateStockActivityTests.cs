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
    public class UpdateStockActivityTests
    {
        private readonly Mock<IBaseRepositoryFactory<StockIngredient>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<StockIngredient>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();

        public UpdateStockActivityTests()
        {
            _repoMock = new Mock<IBaseRepository<StockIngredient>>();
            _repoMock.Setup(x => x.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new StockIngredient { Id = "test" }));
            _repoMock.Setup(x => x.TryUpdateWithRetry(It.IsAny<StockIngredient>(), It.IsAny<Action<StockIngredient>>(), It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<StockIngredient>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_Id_Is_Null()
        {
            var result = await UpdateStockActivity.Run((null, 1), _repoFactoryMock.Object, _logger);

            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_Delta_Is_0()
        {
            var result = await UpdateStockActivity.Run(("1", 0), _repoFactoryMock.Object, _logger);

            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_True_When_Parameters_Are_Ok()
        {
            var result = await UpdateStockActivity.Run(("1", 1), _repoFactoryMock.Object, _logger);

            Assert.True(result);
        }
    }
}