using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
[ExcludeFromCodeCoverage]
public class StateMachineDefinitionsSearchServiceStub : IStateMachineDefinitionSearchService
{
    public Task<SearchStateMachineDefinitionResult> SearchAsync(SearchStateMachineDefinitionCriteria criteria, bool clone = true)
    {
        var result = new SearchStateMachineDefinitionResult();
        result.Results = _stateMachineDefinitions
            .Where(x => criteria.ObjectIds.Contains(x.Id)).ToList();
        result.TotalCount = result.Results.Count;

        return Task.FromResult(result);
    }

    private StateMachineDefinition[] _stateMachineDefinitions =
    [
        new StateMachineDefinition
        {
            Id = "TestStateMachineDefinitionId1",
            Name = "TestStateMachineDefinitionName1",
            EntityType = "TestEntityType1",
        },
        new StateMachineDefinition
        {
            Id = "TestStateMachineDefinitionId2",
            Name = "TestStateMachineDefinitionName2",
            EntityType = "TestEntityType2",
        },
        new StateMachineDefinition
        {
            Id = "TestStateMachineDefinitionId3",
            Name = "TestStateMachineDefinitionName3",
            EntityType = "TestEntityType3",
        },
    ];
}
