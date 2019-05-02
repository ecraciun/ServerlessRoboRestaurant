using Core.Entities;
using Core.Services;
using Microsoft.Azure.Documents;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Core.Tests.Services
{
    public class CosmosDbBaseRepositoryTests
    {
        private readonly Mock<IDocumentClient> _mockDocumentClient;

        public CosmosDbBaseRepositoryTests()
        {
            _mockDocumentClient = new Mock<IDocumentClient>();
        }

        [Fact]
        public void Constructor_Should_Throw_When_Null_DocumentClient()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new CosmosDbBaseRepository<EntityBase>(null, null));

            Assert.NotNull(ex);
        }

        [Fact]
        public void Constructor_Should_Throw_When_Null_CollectionId()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new CosmosDbBaseRepository<EntityBase>(_mockDocumentClient.Object, null));

            Assert.NotNull(ex);
        }

        [Fact]
        public void Constructor_Ok_Test()
        {
            var target = 
                new CosmosDbBaseRepository<EntityBase>(_mockDocumentClient.Object, "abc");

            Assert.NotNull(target);
        }

        [Fact]
        public async Task Delete_Throws_When_Id_Is_Null()
        {
            var target =
                new CosmosDbBaseRepository<EntityBase>(_mockDocumentClient.Object, "abc");

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                target.DeleteAsync(null));
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task Get_Throws_When_Id_Is_Null()
        {
            var target =
                new CosmosDbBaseRepository<EntityBase>(_mockDocumentClient.Object, "abc");

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                target.GetAsync(null));
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task Upsert_Throws_When_Entity_Is_Null()
        {
            var target =
                new CosmosDbBaseRepository<EntityBase>(_mockDocumentClient.Object, "abc");

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                target.UpsertAsync(null));
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task Upsert_Throws_When_EntityId_Is_Null()
        {
            var target =
                new CosmosDbBaseRepository<EntityBase>(_mockDocumentClient.Object, "abc");

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                target.UpsertAsync(new EntityBase()));
            Assert.NotNull(ex);
        }
    }
}