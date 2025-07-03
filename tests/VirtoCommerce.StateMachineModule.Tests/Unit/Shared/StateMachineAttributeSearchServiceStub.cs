using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
[ExcludeFromCodeCoverage]
public class StateMachineAttributeSearchServiceStub : IStateMachineAttributeSearchService
{
    public Task<SearchStateMachineAttributeResult> SearchAsync(SearchStateMachineAttributeCriteria criteria, bool clone = true)
    {
        var result = new SearchStateMachineAttributeResult();
        result.Results = _stateMachineAttributes
            .Where(x => x.DefinitionId == criteria.DefinitionId).ToList();
        result.TotalCount = result.Results.Count;

        return Task.FromResult(result);
    }

    private StateMachineAttribute[] _stateMachineAttributes =
[
    new StateMachineAttribute
        {
            DefinitionId = "TestStateMachineDefinitionId1",
            Item = "TestStateMachineDefinitionStateName1",
            AttributeKey = "Color",
            Value = "Red",
        },
        new StateMachineAttribute
        {
            DefinitionId = "TestStateMachineDefinitionId1",
            Item = "TestStateMachineDefinitionStateName2",
            AttributeKey = "Color",
            Value = "Green",
        },
        new StateMachineAttribute
        {
            DefinitionId = "TestStateMachineDefinitionId1",
            Item = "TestStateMachineDefinitionTransition1",
            AttributeKey = "Icon",
            Value = "fa-icon",
        },
    ];
}
