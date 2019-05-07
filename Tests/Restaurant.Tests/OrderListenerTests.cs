using Core;
using Core.Entities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestsCommon;
using Xunit;

namespace Restaurant.Tests
{
    public class OrderListenerTests
    {
        private readonly ILogger _logger = TestFactory.CreateLogger(LoggerTypes.List);
        private readonly Mock<DurableOrchestrationClientBase> _contextMock;
        private const string TestOrderOrchestratorId = "test";

        public OrderListenerTests()
        {
            _contextMock = new Mock<DurableOrchestrationClientBase>();
            _contextMock.Setup(x => x.StartNewAsync(Constants.InventoryCheckerEternalOrchestratorFunctionName,
                Constants.InventoryCheckerOrchestratorId, null))
                .ReturnsAsync(Constants.InventoryCheckerOrchestratorId);
            _contextMock.Setup(x => x.PurgeInstanceHistoryAsync(Constants.InventoryCheckerEternalOrchestratorFunctionName))
                .ReturnsAsync(new PurgeHistoryResult(1));
            _contextMock.Setup(x => x.StartNewAsync(Constants.OrderOrchestratorFunctionName, It.IsAny<Order>()))
                .ReturnsAsync(TestOrderOrchestratorId);
        }

        private Document GetDocumentFromOrder(Order order)
        {
            Document result = new Document();

            result.Id = order.Id;
            result.SetPropertyValue(nameof(order.Status), order.Status);
            result.SetPropertyValue(nameof(order.LastModifiedUtc), order.LastModifiedUtc);
            result.SetPropertyValue(nameof(order.OrchestaratorId), order.OrchestaratorId);
            result.SetPropertyValue(nameof(order.OrderItems), order.OrderItems);
            result.SetPropertyValue(nameof(order.TimePlacedUtc), order.TimePlacedUtc);

            return result;
        }

        [Theory]
        [InlineData(OrchestrationRuntimeStatus.Running)]
        [InlineData(OrchestrationRuntimeStatus.Pending)]
        [InlineData(OrchestrationRuntimeStatus.ContinuedAsNew)]
        [InlineData(OrchestrationRuntimeStatus.Completed)]
        public async Task Run_Should_Not_Start_A_New_InventoryCheckerOrchestrator_When_Status_Is(OrchestrationRuntimeStatus status)
        {
            _contextMock.Setup(x => x.GetStatusAsync(Constants.InventoryCheckerOrchestratorId))
                .ReturnsAsync(new DurableOrchestrationStatus
                {
                    RuntimeStatus = status
                });

            await OrderListener.Run(new List<Document>(), _contextMock.Object, _logger);

            _contextMock.Verify(x => x.StartNewAsync(Constants.InventoryCheckerEternalOrchestratorFunctionName,
                Constants.InventoryCheckerOrchestratorId, null), Times.Never);
        }

        [Theory]
        [InlineData(OrchestrationRuntimeStatus.Canceled)]
        [InlineData(OrchestrationRuntimeStatus.Failed)]
        [InlineData(OrchestrationRuntimeStatus.Terminated)]
        public async Task Run_Should_Start_A_New_InventoryCheckerOrchestrator_When_Status_Is(OrchestrationRuntimeStatus status)
        {
            _contextMock.Setup(x => x.GetStatusAsync(Constants.InventoryCheckerOrchestratorId))
                .ReturnsAsync(new DurableOrchestrationStatus
                {
                    RuntimeStatus = status
                });

            await OrderListener.Run(new List<Document>(), _contextMock.Object, _logger);

            _contextMock.Verify(x => x.StartNewAsync(Constants.InventoryCheckerEternalOrchestratorFunctionName,
                Constants.InventoryCheckerOrchestratorId, null), Times.Once);
        }

        [Fact]
        public async Task Run_Should_Start_A_New_InventoryCheckerOrchestrator_When_Status_Is_Null()
        {
            _contextMock.Setup(x => x.GetStatusAsync(Constants.InventoryCheckerOrchestratorId))
                .Returns(Task.FromResult((DurableOrchestrationStatus)null));

            await OrderListener.Run(new List<Document>(), _contextMock.Object, _logger);

            _contextMock.Verify(x => x.StartNewAsync(Constants.InventoryCheckerEternalOrchestratorFunctionName,
                Constants.InventoryCheckerOrchestratorId, null), Times.Once);
        }

        [Fact]
        public async Task Run_Should_Start_A_New_OrderOrchestrator_When_Order_Is_Valid()
        {
            var input = new List<Document>();
            var now = DateTime.UtcNow;
            var valid = new Order
            {
                Id = Guid.NewGuid().ToString(),
                LastModifiedUtc = now,
                OrchestaratorId = null,
                Status = OrderStatus.New,
                TimePlacedUtc = now,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem()
                }
            };
            var differentDates = new Order
            {
                Id = Guid.NewGuid().ToString(),
                LastModifiedUtc = now,
                OrchestaratorId = null,
                Status = OrderStatus.New,
                TimePlacedUtc = now.Subtract(TimeSpan.FromMinutes(1)),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem()
                }
            };
            var differentStatus = new Order
            {
                Id = Guid.NewGuid().ToString(),
                LastModifiedUtc = now,
                OrchestaratorId = null,
                Status = OrderStatus.Preparing,
                TimePlacedUtc = now,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem()
                }
            };
            var emptyOrderedItems = new Order
            {
                Id = Guid.NewGuid().ToString(),
                LastModifiedUtc = now,
                OrchestaratorId = null,
                Status = OrderStatus.New,
                TimePlacedUtc = now,
                OrderItems = new List<OrderItem>()
            };

            input.Add(GetDocumentFromOrder(valid));
            input.Add(GetDocumentFromOrder(differentDates));
            input.Add(GetDocumentFromOrder(differentStatus));
            input.Add(GetDocumentFromOrder(emptyOrderedItems));

            await OrderListener.Run(input, _contextMock.Object, _logger);

            _contextMock.Verify(
                x => x.StartNewAsync(Constants.OrderOrchestratorFunctionName, It.IsAny<Order>()), Times.Once);

            var listLogger = _logger as ListLogger;
            Assert.True(listLogger.Logs.Contains($"Order {valid.Id} was picked up by {TestOrderOrchestratorId}"));
        }
    }
}