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
public class SearchStateMachineInstancesQueryTests
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
        var query = new SearchStateMachineInstancesQuery
        {
            ObjectIds = ["TestStateMachineInstanceId1"]
        };
        var cancellationToken = new CancellationToken();

        // Act
        var searchStateMachineInstanceResult = await GetQueryHandler().Handle(query, cancellationToken);

        // Assert
        searchStateMachineInstanceResult.Results.Should().NotBeNullOrEmpty();
        searchStateMachineInstanceResult.TotalCount.Should().Be(1);
    }

    private SearchStateMachineInstancesQueryHandler GetQueryHandler()
    {
        IStateMachineInstanceSearchService stateMachineInstancesSearchServiceStub = new StateMachineInstancesSearchServiceStub();
        return new SearchStateMachineInstancesQueryHandler(stateMachineInstancesSearchServiceStub);
    }


}
