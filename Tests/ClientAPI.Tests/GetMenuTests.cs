using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TestsCommon;
using Xunit;

namespace ClientAPI.Tests
{
    public class GetMenuTests
    {
        private readonly Mock<IBaseRepositoryFactory<Dish>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<Dish>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();
        private readonly IList<Dish> _menu = new List<Dish>();

        public GetMenuTests()
        {
            CreateTestData();

            _repoMock = new Mock<IBaseRepository<Dish>>();
            _repoMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(_menu));
            _repoMock.Setup(x => x.Async(It.IsAny<Expression<Func<Dish, bool>>>()))
                .Returns(Task.FromResult(_menu.Take(2).ToList() as IList<Dish>));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<Dish>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        private void CreateTestData()
        {
            _menu.Add(new Dish
            {
                Description = "abc",
                Id = "1",
                ImageUrl = "abc",
                Price = 10m,
                Title = "Test 1",
                Type = DishType.Beverage
            });

            _menu.Add(new Dish
            {
                Description = "abc",
                Id = "2",
                ImageUrl = "abc",
                Price = 10m,
                Title = "Test 1",
                Type = DishType.Desert
            });

            _menu.Add(new Dish
            {
                Description = "abc",
                Id = "3",
                ImageUrl = "abc",
                Price = 10m,
                Title = "Test 1",
                Type = DishType.Main
            });

            _menu.Add(new Dish
            {
                Description = "abc",
                Id = "4",
                ImageUrl = "abc",
                Price = 10m,
                Title = "Test 1",
                Type = DishType.Side
            });

            _menu.Add(new Dish
            {
                Description = "abc",
                Id = "5",
                ImageUrl = "abc",
                Price = 10m,
                Title = "Test 1",
                Type = DishType.Starter
            });

            _menu.Add(new Dish
            {
                Description = "abc",
                Id = "6",
                ImageUrl = "abc",
                Price = 10m,
                Title = "Test 1",
                Type = DishType.Main
            });
        }

        [Fact]
        public async Task GetMenu_With_No_QueryString_Returns_All()
        {
            var request = TestFactory.CreateHttpRequest();
            var response = await GetMenu.Run(request, _logger, _repoFactoryMock.Object) as JsonResult;
            Assert.NotNull(response);
            var data = response.Value as List<Dish>;
            Assert.NotNull(data);
            Assert.NotEmpty(data);
            Assert.Equal(_menu.Count, data.Count);
        }

        [Fact]
        public async Task GetMenu_With_Valid_QueryString_Returns_Subset()
        {
            var request = TestFactory.CreateHttpRequest(query:
                new Dictionary<string, string> { { "type", DishType.Main.ToString() } });
            var response = await GetMenu.Run(request, _logger, _repoFactoryMock.Object) as JsonResult;
            Assert.NotNull(response);
            var data = response.Value as List<Dish>;
            Assert.NotNull(data);
            Assert.NotEmpty(data);
            Assert.Equal(2, data.Count);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("100")]
        public async Task GetMenu_With_Invalid_QueryString_Returns_Nothing(string queryStringValue)
        {
            var request = TestFactory.CreateHttpRequest(query:
                new Dictionary<string, string> { { "type", queryStringValue } });
            var response = await GetMenu.Run(request, _logger, _repoFactoryMock.Object) as JsonResult;
            Assert.NotNull(response);
            Assert.Null(response.Value);
        }
    }
}