using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TestsCommon;
using Xunit;

namespace Restaurant.Tests
{
    public class SupplierFinderActivityTests
    {
        private readonly Mock<IBaseRepositoryFactory<Supplier>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<Supplier>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();
        private List<Supplier> _suppliers;
        private const string TestIngredient = "beer";

        public SupplierFinderActivityTests()
        {
            CreateTestData();

            _repoMock = new Mock<IBaseRepository<Supplier>>();
            _repoMock.Setup(x => x.GetWhereAsync(It.IsAny<Expression<Func<Supplier, bool>>>()))
                .Returns(Task.FromResult(_suppliers as IList<Supplier>));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<Supplier>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        private void CreateTestData()
        {
            _suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Id = "1",
                    Name = "Test 1",
                    TimeToDelivery = 10,
                    IngredientsForSale = new List<SupplierIngredient>
                    {
                        new SupplierIngredient
                        {
                            Name = TestIngredient,
                            UnitPrice = 40
                        }
                    }
                },
                new Supplier
                {
                    Id = "2",
                    Name = "Test 2",
                    TimeToDelivery = 20,
                    IngredientsForSale = new List<SupplierIngredient>
                    {
                        new SupplierIngredient
                        {
                            Name = TestIngredient,
                            UnitPrice = 30
                        }
                    }
                },
                new Supplier
                {
                    Id = "3",
                    Name = "Test 3",
                    TimeToDelivery = 30,
                    IngredientsForSale = new List<SupplierIngredient>
                    {
                        new SupplierIngredient
                        {
                            Name = TestIngredient,
                            UnitPrice = 20
                        }
                    }
                },
                new Supplier
                {
                    Id = "4",
                    Name = "Test 4",
                    TimeToDelivery = 40,
                    IngredientsForSale = new List<SupplierIngredient>
                    {
                        new SupplierIngredient
                        {
                            Name = TestIngredient,
                            UnitPrice = 10
                        }
                    }
                }
            };
        }

        [Fact]
        public async Task Run_Should_Return_Null_When_Request_Is_Null()
        {
            var result = await SupplierFinderActivity.Run(null, _repoFactoryMock.Object, _logger);
            Assert.Null(result);
        }

        [Fact]
        public async Task Run_Should_Return_Null_When_IngredientName_Is_Null()
        {
            var result = await SupplierFinderActivity.Run(new SupplierQueryRequest
            {
                IngredientName = null,
                QueryStrategy = SupplierQueryStrategy.OptimizeCost
            }, _repoFactoryMock.Object, _logger);
            Assert.Null(result);
        }

        [Fact]
        public async Task Run_Should_Return_Null_When_No_Suppliers_Are_Found()
        {
            _repoMock.Setup(x => x.GetWhereAsync(It.IsAny<Expression<Func<Supplier, bool>>>()))
                .Returns(Task.FromResult<IList<Supplier>>(null));
            var result = await SupplierFinderActivity.Run(new SupplierQueryRequest
            {
                IngredientName = TestIngredient,
                QueryStrategy = SupplierQueryStrategy.OptimizeCost
            }, _repoFactoryMock.Object, _logger);
            Assert.Null(result);
        }

        [Fact]
        public async Task Run_Should_Return_Correct_Supplier_When_Strategy_Optimizez_Cost()
        {
            var result = await SupplierFinderActivity.Run(new SupplierQueryRequest
            {
                IngredientName = TestIngredient,
                QueryStrategy = SupplierQueryStrategy.OptimizeCost
            }, _repoFactoryMock.Object, _logger);

            Assert.NotNull(result);
            Assert.Equal(TestIngredient, result.IngredientName);
            Assert.Equal("4", result.SupplierId);
            Assert.Equal(10, result.UnitPrice);
            Assert.Equal(40, result.TimeToDelivery);
        }

        [Fact]
        public async Task Run_Should_Return_Correct_Supplier_When_Strategy_Optimizez_Speed()
        {
            var result = await SupplierFinderActivity.Run(new SupplierQueryRequest
            {
                IngredientName = TestIngredient,
                QueryStrategy = SupplierQueryStrategy.OptimizeDelivery
            }, _repoFactoryMock.Object, _logger);

            Assert.NotNull(result);
            Assert.Equal(TestIngredient, result.IngredientName);
            Assert.Equal("1", result.SupplierId);
            Assert.Equal(40, result.UnitPrice);
            Assert.Equal(10, result.TimeToDelivery);
        }
    }
}