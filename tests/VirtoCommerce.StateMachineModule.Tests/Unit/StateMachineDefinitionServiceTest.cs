using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Data.Models;
using VirtoCommerce.StateMachineModule.Data.Repositories;
using VirtoCommerce.StateMachineModule.Data.Services;
using VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;
[ExcludeFromCodeCoverage]
public class StateMachineDefinitionServiceTest
{
    private static readonly string _testStateMachineDefinitionId = "TestStateMachineDefinitionId";
    private static readonly string _testStateMachineDefinitionName = "TestStateMachineDefinitionName";
    private static readonly string _testStateMachineDefinitionEntityType = "TestStateMachineDefinitionEntityType";
    private static readonly StateMachineDefinitionEntity _stateMachineDefinitionEntity = new()
    {
        Id = _testStateMachineDefinitionId,
        Name = _testStateMachineDefinitionName,
        EntityType = _testStateMachineDefinitionEntityType,
        IsActive = true,
        StatesSerialized = TestHepler.LoadArrayFromJsonFile("testStateMachineDefinition.json").ToString(),
    };

    [Fact]
    public async Task GetActiveStateMachineDefinition_ExistedEntityType_ReturnsValue()
    {
        // Arrange
        StateMachineRepositoryMock stateMachineRepositoryMock = new();
        stateMachineRepositoryMock.Add(_stateMachineDefinitionEntity);
        var stateMachineDefinitionService = GetStateMachineDefinitionService(stateMachineRepositoryMock);

        // Act
        var actualStateMachineDefinition = await stateMachineDefinitionService.GetActiveStateMachineDefinitionAsync(_testStateMachineDefinitionEntityType);

        // Assertion
        actualStateMachineDefinition.Should().NotBeNull();
        actualStateMachineDefinition.Id.Should().Be(_stateMachineDefinitionEntity.Id);
    }

    [Fact]
    public async Task GetActiveStateMachineDefinition_NonExistedEntityType_ReturnsNull()
    {
        // Arrange
        var stateMachineDefinitionService = GetStateMachineDefinitionService();

        // Act
        var actualStateMachineDefinition = await stateMachineDefinitionService.GetActiveStateMachineDefinitionAsync(_testStateMachineDefinitionEntityType);

        // Assertion
        actualStateMachineDefinition.Should().BeNull();
    }

    [Fact]
    public async Task SaveStateMachineDefinition_NotNull_ValueSaves()
    {
        // Arrange
        var stateMachineDefinitionService = GetStateMachineDefinitionService();
        var stateMachineDefinition = _stateMachineDefinitionEntity.ToModel(new StateMachineDefinition());

        // Act
        await stateMachineDefinitionService.SaveStateMachineDefinitionAsync(stateMachineDefinition);
        var savedStateMachineDefinition = await stateMachineDefinitionService.GetByIdAsync(_testStateMachineDefinitionId);

        // Assertion
        savedStateMachineDefinition.Should().NotBeNull();
        savedStateMachineDefinition.Id.Should().Be(stateMachineDefinition.Id);
    }

    private StateMachineDefinitionService GetStateMachineDefinitionService(
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

        var eventPublisherMock = new Mock<IEventPublisher>();

        var stateMachineDefinitionService = new StateMachineDefinitionService
        (
            () => stateMachineRepositoryMock,
            platformMemoryCache,
            eventPublisherMock.Object
        );

        return stateMachineDefinitionService;
    }
}
