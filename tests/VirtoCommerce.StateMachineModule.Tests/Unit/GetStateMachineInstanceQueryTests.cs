using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data.Queries;
using VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;
[ExcludeFromCodeCoverage]
public class GetStateMachineInstanceQueryTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsData()
    {
        // Arrange
        var query = new GetStateMachineInstanceQuery
        {
            StateMachineInstanceId = "TestStateMachineInstanceId"
        };
        var cancellationToken = new CancellationToken();

        // Act
        var stateMachineInstance = await GetQueryHandler().Handle(query, cancellationToken);

        // Assert
        stateMachineInstance.Id.Should().Be(query.StateMachineInstanceId);
    }

    private GetStateMachineInstanceQueryHandler GetQueryHandler()
    {
        IStateMachineInstanceService stateMachineInstanceService = new StateMachineInstanceServiceStub();
        return new GetStateMachineInstanceQueryHandler(stateMachineInstanceService);
    }
}
