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
    public class GetSupplierOrdersTests
    {
        private readonly Mock<IBaseRepositoryFactory<SupplierOrder>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<SupplierOrder>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();
        private readonly IList<SupplierOrder> _orders = new List<SupplierOrder>();

        public GetSupplierOrdersTests()
        {
            CreateTestData();

            _repoMock = new Mock<IBaseRepository<SupplierOrder>>();
            _repoMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(_orders));
            _repoMock.Setup(x => x.Async(It.IsAny<Expression<Func<SupplierOrder, bool>>>()))
                .Returns(Task.FromResult(_orders.Take(1).ToList() as IList<SupplierOrder>));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<SupplierOrder>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        private void CreateTestData()
        {
            _orders.Add(new SupplierOrder
            {
                CreatedAt = DateTime.UtcNow,
                Id = "1",
                LastModified = DateTime.UtcNow,
                Status = SupplierOrderStatus.Created,
                SupplierId = "abc"
            });
            _orders.Add(new SupplierOrder
            {
                CreatedAt = DateTime.UtcNow,
                Id = "2",
                LastModified = DateTime.UtcNow,
                Status = SupplierOrderStatus.Delivered,
                SupplierId = "abc"
            });
            _orders.Add(new SupplierOrder
            {
                CreatedAt = DateTime.UtcNow,
                Id = "3",
                LastModified = DateTime.UtcNow,
                Status = SupplierOrderStatus.Processing,
                SupplierId = "abc"
            });
        }

        [Fact]
        public async Task GetSupplierOrders_With_No_QueryString_Returns_All()
        {
            var request = TestFactory.CreateHttpRequest();
            var response = await GetSupplierOrders.Run(request, _repoFactoryMock.Object, _logger) as JsonResult;
            Assert.NotNull(response);
            var data = response.Value as List<SupplierOrder>;
            Assert.NotNull(data);
            Assert.NotEmpty(data);
            Assert.Equal(_orders.Count, data.Count);
        }

        [Fact]
        public async Task GetSupplierOrders_With_Valid_QueryString_Returns_Subset()
        {
            var request = TestFactory.CreateHttpRequest(query:
                new Dictionary<string, string> { { "status", SupplierOrderStatus.Created.ToString() } });
            var response = await GetSupplierOrders.Run(request, _repoFactoryMock.Object, _logger) as JsonResult;
            Assert.NotNull(response);
            var data = response.Value as List<SupplierOrder>;
            Assert.NotNull(data);
            Assert.NotEmpty(data);
            Assert.Single(data);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("100")]
        public async Task GetSupplierOrders_With_Invalid_QueryString_Returns_Nothing(string queryStringValue)
        {
            var request = TestFactory.CreateHttpRequest(query:
                new Dictionary<string, string> { { "status", queryStringValue } });
            var response = await GetSupplierOrders.Run(request, _repoFactoryMock.Object, _logger) as JsonResult;
            Assert.NotNull(response);
            Assert.Null(response.Value);
        }
    }
}