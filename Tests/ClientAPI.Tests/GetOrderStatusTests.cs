using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestsCommon;
using Xunit;

namespace ClientAPI.Tests
{
    public class GetOrderStatusTests
    {
        private readonly Mock<IBaseRepositoryFactory<Order>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<Order>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();
        private readonly Order _dummyOrder = new Order
        {
            Id = "1",
            Status = OrderStatus.Preparing,
            LastModifiedUtc = DateTime.UtcNow,
            TimePlacedUtc = DateTime.UtcNow
        };

        public GetOrderStatusTests()
        {
            _repoMock = new Mock<IBaseRepository<Order>>();
            _repoMock.Setup(x => x.Get(It.IsAny<string>())).Returns(Task.FromResult(_dummyOrder));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<Order>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        [Fact]
        public async Task GetOrderStauts_Returns_BadRequest_When_No_QueryString()
        {
            var request = TestFactory.CreateHttpRequest();
            var response = await GetOrderStatus.Run(request, _repoFactoryMock.Object, _logger) 
                as BadRequestObjectResult;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetOrderStauts_Returns_BadRequest_When_Invalid_QueryString()
        {
            var request = TestFactory.CreateHttpRequest(query:
                new Dictionary<string, string> { { "orderId", "abc" } });
            var response = await GetOrderStatus.Run(request, _repoFactoryMock.Object, _logger)
                as BadRequestObjectResult;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetOrderStauts_Returns_Status_When_Ok_QueryString()
        {
            var request = TestFactory.CreateHttpRequest(query:
                new Dictionary<string, string> { { "orderId", Guid.NewGuid().ToString() } });
            var response = await GetOrderStatus.Run(request, _repoFactoryMock.Object, _logger)
                as JsonResult;
            Assert.NotNull(response);
            var data = (OrderStatus)response.Value;
            Assert.Equal(OrderStatus.Preparing, data);
        }
    }
}