using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TestsCommon;
using Xunit;

namespace ClientAPI.Tests
{
    public class PlaceOrderTests
    {
        private readonly Mock<IBaseRepositoryFactory<Order>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<Order>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();
        private readonly Order _dummyOrder = new Order
        {
            Id = "1",
            Status = OrderStatus.New,
            LastModifiedUtc = DateTime.UtcNow,
            TimePlacedUtc = DateTime.UtcNow,
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    DishId = "1",
                    Quantity = 2
                }
            }
        };

        public PlaceOrderTests()
        {
            _repoMock = new Mock<IBaseRepository<Order>>();
            _repoMock.Setup(x => x.Add(It.IsAny<Order>())).Returns(Task.FromResult(_dummyOrder.Id));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<Order>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        [Fact]
        public async Task PlaceOrder_Returns_BadRequest_When_Body_Is_Empty()
        {
            var request = TestFactory.CreateHttpRequest(method: "post");
            var response = await PlaceOrder.Run(request, _repoFactoryMock.Object, _logger)
                as BadRequestResult;
            Assert.NotNull(response);
        }

        [Theory]
        [InlineData("test")]
        [InlineData("{\"Prop\":\"test\"}")]
        public async Task PlaceOrder_Returns_BadRequest_When_Body_Is_Invalid(string bodyContents)
        {
            var request = TestFactory.CreateHttpRequest(method: "post");

            using (var memoryStream = new MemoryStream())
            {
                using (TextWriter textWriter = new StreamWriter(memoryStream))
                {
                    textWriter.Write(bodyContents);
                    textWriter.Flush();
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    request.Body = memoryStream;

                    var response = await PlaceOrder.Run(request, _repoFactoryMock.Object, _logger)
                    as BadRequestObjectResult;
                    Assert.NotNull(response);
                }
            }
        }

        [Fact]
        public async Task PlaceOrder_Returns_CreatedRequest_When_Body_Is_Ok()
        {
            var request = TestFactory.CreateHttpRequest(method: "post");

            using (var memoryStream = new MemoryStream())
            {
                using (TextWriter textWriter = new StreamWriter(memoryStream))
                {
                    textWriter.Write(JsonConvert.SerializeObject(_dummyOrder));
                    textWriter.Flush();
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    request.Body = memoryStream;

                    var response = await PlaceOrder.Run(request, _repoFactoryMock.Object, _logger)
                    as CreatedResult;
                    Assert.NotNull(response);
                    Assert.Equal($"api/{nameof(GetOrderStatus)}/1", response.Location);
                }
            }
        }
    }
}