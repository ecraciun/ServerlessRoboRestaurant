using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestsCommon;
using Xunit;

namespace Restaurant.Tests
{
    public class CreateSupplierOrderActivityTests
    {
        private readonly Mock<IBaseRepositoryFactory<SupplierOrder>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<SupplierOrder>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();
        private const string TestOrderId = "test_order";
        private const string TestSupplierId = "test_supplier";
        private readonly SupplierOrder _testOrder = new SupplierOrder
        {
            DeliveryETAInSeconds = 1,
            Id = TestOrderId,
            SupplierId = TestSupplierId,
            Status = SupplierOrderStatus.Processing,
            OrderedItems = new List<SupplierOrderIngredientItem>
            {
                new SupplierOrderIngredientItem
                {
                    Name = "abc",
                    Quantity = 1
                }
            }
        };

        public CreateSupplierOrderActivityTests()
        {
            _repoMock = new Mock<IBaseRepository<SupplierOrder>>();
            _repoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(_testOrder));
            _repoMock.Setup(x => x.AddAsync(It.IsAny<SupplierOrder>())).Returns(Task.FromResult(TestOrderId));
            _repoMock.Setup(x => x.TryUpdateWithRetry(
                It.IsAny<SupplierOrder>(), It.IsAny<Action<SupplierOrder>>(), It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<SupplierOrder>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_Order_Is_Null()
        {
            var result = await CreateSupplierOrderActivity.Run(null, _repoFactoryMock.Object, _logger);
            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_OrderedItems_Is_Empty()
        {
            var result = await CreateSupplierOrderActivity.Run(new SupplierOrder
            {
                SupplierId = "abc",
                OrderedItems = new List<SupplierOrderIngredientItem>()
            }, _repoFactoryMock.Object, _logger);
            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_False_When_SupplierId_Is_Empty()
        {
            var result = await CreateSupplierOrderActivity.Run(new SupplierOrder
            {
                SupplierId = string.Empty,
                OrderedItems = new List<SupplierOrderIngredientItem>
                {
                    new SupplierOrderIngredientItem
                    {
                        Name = "abc",
                        Quantity = 1
                    }
                }
            }, _repoFactoryMock.Object, _logger);
            Assert.False(result);
        }

        [Fact]
        public async Task Run_Should_Return_True_When_Order_Is_Ok()
        {
            var result = await CreateSupplierOrderActivity.Run(_testOrder, _repoFactoryMock.Object, _logger);

            Assert.True(result);

            _repoMock.Verify(x => x.AddAsync(_testOrder), Times.Once);
        }
    }
}