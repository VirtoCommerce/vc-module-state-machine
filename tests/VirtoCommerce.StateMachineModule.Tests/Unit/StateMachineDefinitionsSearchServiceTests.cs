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
public class StateMachineDefinitionsSearchServiceTests
{
    [Theory]
    [MemberData(nameof(StateMachineSearchTestInput))]
    public async Task StateMachineSearchTest(string entityType, int expectedMessagesCount)
    {
        // Arrange
        StateMachineRepositoryMock stateMachineRepositoryMock = new();
        foreach (var stateMachineDefinitionEntity in _stateMachineDefinitionEntities)
        {
            stateMachineRepositoryMock.Add(stateMachineDefinitionEntity);
        }

        var stateMachineDefinitionSearchService = GetStateMachineDefinitionsSearchService(stateMachineRepositoryMock);

        var criteria = new SearchStateMachineDefinitionCriteria();
        criteria.ObjectTypes = [entityType];

        // Act
        var actualSearchResult = await stateMachineDefinitionSearchService.SearchAsync(criteria);

        // Assertion
        actualSearchResult.Should().NotBeNull();
        actualSearchResult.TotalCount.Should().Be(expectedMessagesCount);
    }

    private StateMachineDefinitionSearchService GetStateMachineDefinitionsSearchService
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

        var stateMachineDefinitionsSearchService = new StateMachineDefinitionSearchService
        (
            () => stateMachineRepositoryMock,
            platformMemoryCache,
            serviceProvider.GetService<IStateMachineDefinitionService>(),
            serviceProvider.GetService<IOptions<CrudOptions>>(),
            serviceProvider.GetService<IStateMachineLocalizationSearchService>(),
            serviceProvider.GetService<IStateMachineAttributeSearchService>()
        );

        return stateMachineDefinitionsSearchService;
    }

    private StateMachineDefinitionEntity[] _stateMachineDefinitionEntities =
    [
        new StateMachineDefinitionEntity
        {
            Id = "TestStateMachineDefinitionId1",
            Name = "TestStateMachineDefinitionName1",
            EntityType = "TestEntityType1",
        },
        new StateMachineDefinitionEntity
        {
            Id = "TestStateMachineDefinitionId2",
            Name = "TestStateMachineDefinitionName2",
            EntityType = "TestEntityType1",
        },
        new StateMachineDefinitionEntity
        {
            Id = "TestStateMachineDefinitionId3",
            Name = "TestStateMachineDefinitionName3",
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
