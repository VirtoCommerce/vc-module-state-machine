using System;
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
    public void Handle_NullCommandRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        // Act
        Action actual = () => GetQueryHandler().Handle(null, cancellationToken).GetAwaiter().GetResult();

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Fact]
    public void Handle_InvalidRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var query = new GetStateMachineInstanceQuery();
        var cancellationToken = new CancellationToken();

        // Act
        Action actual = () => GetQueryHandler().Handle(query, cancellationToken).GetAwaiter().GetResult();

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

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
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService = new StateMachineLocalizationSearchServiceStub();
        IStateMachineAttributeSearchService stateMachineAttributeSearchService = new StateMachineAttributeSearchServiceStub();
        return new GetStateMachineInstanceQueryHandler(stateMachineInstanceService, stateMachineLocalizationSearchService, stateMachineAttributeSearchService);
    }
}
