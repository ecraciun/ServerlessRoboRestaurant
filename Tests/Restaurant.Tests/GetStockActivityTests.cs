using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestsCommon;
using Xunit;

namespace Restaurant.Tests
{
    public class GetStockActivityTests
    {
        private readonly Mock<IBaseRepositoryFactory<StockIngredient>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<StockIngredient>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();

        public GetStockActivityTests()
        {
            _repoMock = new Mock<IBaseRepository<StockIngredient>>();
            _repoMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(
                new List<StockIngredient>()
                {
                    new StockIngredient()
                } as IList<StockIngredient>));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<StockIngredient>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        [Fact]
        public async Task Run_Should_Return_Collection()
        {
            var result = await GetStockActivity.Run("test", _repoFactoryMock.Object, _logger);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}
