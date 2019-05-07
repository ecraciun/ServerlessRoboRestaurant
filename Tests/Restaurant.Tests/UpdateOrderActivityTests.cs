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
    public class UpdateOrderActivityTests
    {
        private readonly Mock<IBaseRepositoryFactory<Order>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<Order>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();
        private const string TestOrchestratorId = "test";

        public UpdateOrderActivityTests()
        {
            _repoMock = new Mock<IBaseRepository<Order>>();
            _repoMock.Setup(x => x.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(
                    new Order
                    {
                        Id = "test",
                        OrchestaratorId = TestOrchestratorId,
                        Status = OrderStatus.New
                    }));
            _repoMock.Setup(x => x.TryUpdateWithRetry(It.IsAny<Order>(), It.IsAny<Action<Order>>(), It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<Order>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_OrderDTO_Is_Null()
        {
            var result = await UpdateOrderActivity.Run(null, _repoFactoryMock.Object, _logger);
            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_OrchestratorId_Is_Null()
        {
            var result = await UpdateOrderActivity.Run(
                new OrderDTO
                {
                    OrchestratorId = null,
                    OrderId = "abc",
                    OrderStatus = OrderStatus.Preparing
                },
                _repoFactoryMock.Object, _logger);
            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_OrderId_Is_Null()
        {
            var result = await UpdateOrderActivity.Run(
                new OrderDTO
                {
                    OrchestratorId = TestOrchestratorId,
                    OrderId = null,
                    OrderStatus = OrderStatus.Preparing
                }, _repoFactoryMock.Object, _logger);
            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_Status_Is_The_Same()
        {
            var result = await UpdateOrderActivity.Run(
                new OrderDTO
                {
                    OrchestratorId = TestOrchestratorId,
                    OrderId = "abc",
                    OrderStatus = OrderStatus.New
                }, _repoFactoryMock.Object, _logger);
            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_False_OrchestratorId_Is_Different()
        {
            var result = await UpdateOrderActivity.Run(
                new OrderDTO
                {
                    OrchestratorId = "abc",
                    OrderId = "abc",
                    OrderStatus = OrderStatus.Preparing
                }, _repoFactoryMock.Object, _logger);
            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_True_When_Params_Are_Ok()
        {
            var result = await UpdateOrderActivity.Run(new OrderDTO
            {
                OrchestratorId = TestOrchestratorId,
                OrderId = "abc",
                OrderStatus = OrderStatus.Preparing
            }, _repoFactoryMock.Object, _logger);
            Assert.True(result);
        }
    }
}