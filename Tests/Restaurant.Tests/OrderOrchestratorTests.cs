using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using TestsCommon;
using Xunit;

namespace Restaurant.Tests
{
    public class OrderOrchestratorTests
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();
        private readonly Mock<DurableOrchestrationContextBase> _contextMock;

        public OrderOrchestratorTests()
        {

        }
    }
}
