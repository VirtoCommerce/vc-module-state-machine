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
public class GetStateMachineDefinitionStatesQueryTests
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
        var query = new GetStateMachineDefinitionStatesQuery();
        var cancellationToken = new CancellationToken();

        // Act
        Action actual = () => GetQueryHandler().Handle(query, cancellationToken).GetAwaiter().GetResult();

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Fact]
    public void Handle_InvalidEntityType_ThrowsInvalidOperationException()
    {
        // Arrange
        var query = new GetStateMachineDefinitionStatesQuery
        {
            EntityType = "InvalidEntityType"
        };
        var cancellationToken = new CancellationToken();

        // Act
        Action actual = () => GetQueryHandler().Handle(query, cancellationToken).GetAwaiter().GetResult();

        // Assertion
        Assert.Throws<InvalidOperationException>(actual);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsData()
    {
        // Arrange
        var query = new GetStateMachineDefinitionStatesQuery
        {
            EntityType = "TestEntityType"
        };
        var cancellationToken = new CancellationToken();

        // Act
        var stateMachineDefinitionStates = await GetQueryHandler().Handle(query, cancellationToken);

        // Assert
        stateMachineDefinitionStates.Should().NotBeNull();
        stateMachineDefinitionStates.Length.Should().Be(1);
    }

    private GetStateMachineDefinitionStatesQueryHandler GetQueryHandler()
    {
        IStateMachineDefinitionService stateMachineDefinitionService = new StateMachineDefinitionServiceStub();
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService = new StateMachineLocalizationSearchServiceStub();
        return new GetStateMachineDefinitionStatesQueryHandler(stateMachineDefinitionService, stateMachineLocalizationSearchService);
    }
}
