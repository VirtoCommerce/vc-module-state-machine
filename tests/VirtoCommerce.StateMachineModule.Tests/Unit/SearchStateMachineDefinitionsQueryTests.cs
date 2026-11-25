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
public class SearchStateMachineDefinitionsQueryTests
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
    public async Task Handle_ValidRequest_ReturnsData()
    {
        // Arrange
        var query = new SearchStateMachineDefinitionsQuery
        {
            ObjectIds = ["TestStateMachineDefinitionId1"]
        };
        var cancellationToken = new CancellationToken();

        // Act
        var searchStateMachineDefinitionResult = await GetQueryHandler().Handle(query, cancellationToken);

        // Assert
        searchStateMachineDefinitionResult.Results.Should().NotBeNullOrEmpty();
        searchStateMachineDefinitionResult.TotalCount.Should().Be(1);
    }

    [Fact]
    public void ToCriteria_CopiesSortAndIsActive()
    {
        // Arrange
        var query = new SearchStateMachineDefinitionsQuery
        {
            Sort = "name:ASC",
            IsActive = true,
            ObjectTypes = ["TestEntityType"],
            Skip = 20,
            Take = 10,
        };

        // Act
        var criteria = query.ToCriteria();

        // Assert
        criteria.Sort.Should().Be("name:ASC");
        criteria.IsActive.Should().BeTrue();
        criteria.ObjectTypes.Should().Equal("TestEntityType");
        criteria.Skip.Should().Be(20);
        criteria.Take.Should().Be(10);
    }

    private SearchStateMachineDefinitionsQueryHandler GetQueryHandler()
    {
        IStateMachineDefinitionSearchService stateMachineDefinitionsSearchServiceStub = new StateMachineDefinitionsSearchServiceStub();
        return new SearchStateMachineDefinitionsQueryHandler(stateMachineDefinitionsSearchServiceStub);
    }


}
