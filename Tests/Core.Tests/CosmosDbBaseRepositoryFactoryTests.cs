using Core.Entities;
using Core.Services;
using System;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Core.Tests
{
    [TestCaseOrderer("TestsCommon.PriorityOrderer", "TestsCommon")]
    public class CosmosDbBaseRepositoryFactoryTests
    {
        [Fact, TestPriority(1)]
        public void GetInstance_Should_Throw_When_EndpointUri_Is_Null()
        {
            var target = new CosmosDbBaseRepositoryFactory<EntityBase>();
            var ex = Assert.Throws<ArgumentNullException>(() => 
                target.GetInstance(null, "abc", "abc"));
            Assert.NotNull(ex);
        }

        [Fact, TestPriority(2)]
        public void GetInstance_Should_Throw_When_Key_Is_Null()
        {
            var target = new CosmosDbBaseRepositoryFactory<EntityBase>();
            var ex = Assert.Throws<ArgumentNullException>(() => 
                target.GetInstance("abc", null, "abc"));
            Assert.NotNull(ex);
        }

        [Fact, TestPriority(3)]
        public void GetInstance_Should_Throw_When_CollectionId_Is_Null()
        {
            var target = new CosmosDbBaseRepositoryFactory<EntityBase>();
            var ex = Assert.Throws<ArgumentNullException>(() => 
                target.GetInstance("abc", "abc", null));
            Assert.NotNull(ex);
        }

        [Fact, TestPriority(4)]
        public void GetInstance_Should_Throw_When_Invalid_Uri()
        {
            var target = new CosmosDbBaseRepositoryFactory<EntityBase>();
            var ex = Assert.Throws<UriFormatException>(() => 
                target.GetInstance("abc", "abc", "abc"));
            Assert.NotNull(ex);
        }

        [Fact, TestPriority(5)]
        public void GetInstance_Should_Throw_When_Invalid_Key()
        {
            var target = new CosmosDbBaseRepositoryFactory<EntityBase>();
            var ex = Assert.Throws<FormatException>(() =>
                target.GetInstance("https://localhost:8081", "abc", "abc"));
            Assert.NotNull(ex);
        }

        [Fact, TestPriority(6)]
        public void GetInstance_Should_Return_New_Repository_Instance_When_Parameters_Are_Valid()
        {
            var target = new CosmosDbBaseRepositoryFactory<EntityBase>();
            var repo =  
                target.GetInstance("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", "abc");
            Assert.NotNull(repo);
        }

        [Fact, TestPriority(7)]
        public void GetInstance_Should_Return_Same_Repository_Instance_Between_Multiple_Calls()
        {
            var target = new CosmosDbBaseRepositoryFactory<EntityBase>();
            var repo1 =
                target.GetInstance("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", "abc");
            var repo2 =
                target.GetInstance("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", "abc");
            Assert.NotNull(repo1);
            Assert.NotNull(repo1);
            Assert.True(repo1 == repo2);
        }
    }
}