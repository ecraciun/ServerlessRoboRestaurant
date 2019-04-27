using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestsCommon;
using Xunit;

namespace BackofficeAPI.Tests
{
    public class GetStockIngredientsTests
    {
        private readonly Mock<IBaseRepositoryFactory<StockIngredient>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<StockIngredient>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();
        private readonly IList<StockIngredient> _stockIngredients = new List<StockIngredient>();

        public GetStockIngredientsTests()
        {
            CreateTestData();

            _repoMock = new Mock<IBaseRepository<StockIngredient>>();
            _repoMock.Setup(x => x.GetAll()).Returns(Task.FromResult(_stockIngredients));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<StockIngredient>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        private void CreateTestData()
        {
            _stockIngredients.Add(new StockIngredient
            {
                Id = "1",
                Name = "Test",
                StockQuantity = 100
            });
            _stockIngredients.Add(new StockIngredient
            {
                Id = "2",
                Name = "Test2",
                StockQuantity = 200
            });
        }

        [Fact]
        public async Task GetStockIngredients_With_No_QueryString_Returns_All()
        {
            var request = TestFactory.CreateHttpRequest();
            var response = await GetStockIngredients.Run(request, _repoFactoryMock.Object, _logger) as JsonResult;
            Assert.NotNull(response);
            var data = response.Value as List<StockIngredient>;
            Assert.NotNull(data);
            Assert.NotEmpty(data);
            Assert.Equal(_stockIngredients.Count, data.Count);
        }
    }
}