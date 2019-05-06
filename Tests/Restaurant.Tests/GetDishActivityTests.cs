using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using TestsCommon;
using Xunit;

namespace Restaurant.Tests
{
    public class GetDishActivityTests
    {
        private readonly Mock<IBaseRepositoryFactory<Dish>> _repoFactoryMock;
        private readonly Mock<IBaseRepository<Dish>> _repoMock;
        private readonly ILogger _logger = TestFactory.CreateLogger();

        public GetDishActivityTests()
        {
            _repoMock = new Mock<IBaseRepository<Dish>>();
            _repoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Dish { Id = "test" }));

            _repoFactoryMock = new Mock<IBaseRepositoryFactory<Dish>>();
            _repoFactoryMock.Setup(x => x.GetInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_repoMock.Object);
        }

        [Fact]
        public async Task Run_Returns_Instance_When_Input_Is_Not_Null()
        {
            var result = await GetDishActivity.Run("abc", _repoFactoryMock.Object, _logger);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Run_Returns_Null_When_Input_Is_Null()
        {
            var result = await GetDishActivity.Run(null, _repoFactoryMock.Object, _logger);

            Assert.Null(result);
        }
    }
}