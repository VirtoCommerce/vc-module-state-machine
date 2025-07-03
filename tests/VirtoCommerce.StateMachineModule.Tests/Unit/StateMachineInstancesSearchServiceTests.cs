using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data.Models;
using VirtoCommerce.StateMachineModule.Data.Repositories;
using VirtoCommerce.StateMachineModule.Data.Services;
using VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;
[ExcludeFromCodeCoverage]
public class StateMachineInstancesSearchServiceTests
{
    [Theory]
    [MemberData(nameof(StateMachineSearchTestInput))]
    public async Task StateMachineSearchTest(string entityType, int expectedMessagesCount)
    {
        // Arrange
        StateMachineRepositoryMock stateMachineRepositoryMock = new();
        foreach (var stateMachineInstanceEntity in _stateMachineInstanceEntities)
        {
            stateMachineRepositoryMock.Add(stateMachineInstanceEntity);
        }

        var stateMachineInstanceSearchService = GetStateMachineInstancesSearchService(stateMachineRepositoryMock);

        var criteria = new SearchStateMachineInstanceCriteria();
        criteria.ObjectType = entityType;

        // Act
        var actualSearchResult = await stateMachineInstanceSearchService.SearchAsync(criteria);

        // Assertion
        actualSearchResult.Should().NotBeNull();
        actualSearchResult.TotalCount.Should().Be(expectedMessagesCount);
    }

    private StateMachineInstanceSearchService GetStateMachineInstancesSearchService
    (
        IStateMachineRepository stateMachineRepositoryMock = null
    )
    {
        var serviceProvider = new ServiceBuilder().GetServiceCollection().BuildServiceProvider();

        if (stateMachineRepositoryMock == null)
        {
            stateMachineRepositoryMock = serviceProvider.GetService<IStateMachineRepository>();
        }

        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

        var stateMachineInstancesSearchService = new StateMachineInstanceSearchService
        (
            () => stateMachineRepositoryMock,
            platformMemoryCache,
            serviceProvider.GetService<IStateMachineInstanceService>(),
            serviceProvider.GetService<IOptions<CrudOptions>>(),
            serviceProvider.GetService<IStateMachineLocalizationSearchService>(),
            serviceProvider.GetService<IStateMachineAttributeSearchService>()
        );

        return stateMachineInstancesSearchService;
    }

    private StateMachineInstanceEntity[] _stateMachineInstanceEntities =
    [
        new StateMachineInstanceEntity
        {
            Id = "TestStateMachineInstanceId1",
            EntityType = "TestEntityType1",
        },
        new StateMachineInstanceEntity
        {
            Id = "TestStateMachineInstanceId2",
            EntityType = "TestEntityType1",
        },
        new StateMachineInstanceEntity
        {
            Id = "TestStateMachineInstanceId3",
            EntityType = "TestEntityType3",
        },
    ];

    public static TheoryData<string, int> StateMachineSearchTestInput()
    {
        return new TheoryData<string, int>
        {
            {
                "TestEntityType1",
                2
            },
            {
                "TestEntityType3",
                1
            }
        };
    }
}
