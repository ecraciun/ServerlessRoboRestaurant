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

namespace BackofficeAPI.Tests
{
    public class GetOrdersTests
    {
        private readonly Mock<IBaseRepositoryFactory<Order>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<Order>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();
        private readonly IList<Order> _orders = new List<Order>();

        public GetOrdersTests()
        {
            CreateTestData();

            _repoMock = new Mock<IBaseRepository<Order>>();
            _repoMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(_orders));
            _repoMock.Setup(x => x.GetWhereAsync(It.IsAny<Expression<Func<Order, bool>>>()))
                .Returns(Task.FromResult(_orders.Take(1).ToList() as IList<Order>));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<Order>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        private void CreateTestData()
        {
            _orders.Add(new Order
            {
                Id = "1",
                Status = OrderStatus.New
            });
            _orders.Add(new Order
            {
                Id = "2",
                Status = OrderStatus.Canceled
            });
            _orders.Add(new Order
            {
                Id = "3",
                Status = OrderStatus.Preparing
            });
            _orders.Add(new Order
            {
                Id = "4",
                Status = OrderStatus.Ready
            });
        }

        [Fact]
        public async Task GetOrders_With_No_QueryString_Returns_All()
        {
            var request = TestFactory.CreateHttpRequest();
            var response = await GetOrders.Run(request, _repoFactoryMock.Object, _logger) as JsonResult;
            Assert.NotNull(response);
            var data = response.Value as List<Order>;
            Assert.NotNull(data);
            Assert.NotEmpty(data);
            Assert.Equal(_orders.Count, data.Count);
        }

        [Fact]
        public async Task GetOrders_With_Valid_QueryString_Returns_Subset()
        {
            var request = TestFactory.CreateHttpRequest(query:
                new Dictionary<string, string> { { "status", OrderStatus.New.ToString() } });
            var response = await GetOrders.Run(request, _repoFactoryMock.Object, _logger) as JsonResult;
            Assert.NotNull(response);
            var data = response.Value as List<Order>;
            Assert.NotNull(data);
            Assert.NotEmpty(data);
            Assert.Single(data);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("100")]
        public async Task GetOrders_With_Invalid_QueryString_Returns_Nothing(string queryStringValue)
        {
            var request = TestFactory.CreateHttpRequest(query:
                new Dictionary<string, string> { { "status", queryStringValue } });
            var response = await GetOrders.Run(request, _repoFactoryMock.Object, _logger) as JsonResult;
            Assert.NotNull(response);
            Assert.Null(response.Value);
        }
    }
}