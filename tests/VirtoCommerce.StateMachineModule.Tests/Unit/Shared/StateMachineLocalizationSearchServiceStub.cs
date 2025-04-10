using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
[ExcludeFromCodeCoverage]
public class StateMachineLocalizationSearchServiceStub : IStateMachineLocalizationSearchService
{
    public Task<SearchStateMachineLocalizationResult> SearchAsync(SearchStateMachineLocalizationCriteria criteria, bool clone = true)
    {
        var result = new SearchStateMachineLocalizationResult();
        result.Results = _stateMachineLocalizations
            .Where(x => x.DefinitionId == criteria.DefinitionId && x.Locale == criteria.Locale).ToList();
        result.TotalCount = result.Results.Count;

        return Task.FromResult(result);
    }

    private StateMachineLocalization[] _stateMachineLocalizations =
[
    new StateMachineLocalization
        {
            DefinitionId = "TestStateMachineDefinitionId1",
            Item = "TestStateMachineDefinitionStateName1",
            Locale = "en-US",
            Value = "State1 EN",
        },
        new StateMachineLocalization
        {
            DefinitionId = "TestStateMachineDefinitionId1",
            Item = "TestStateMachineDefinitionStateName2",
            Locale = "en-US",
            Value = "State2 EN",
        },
        new StateMachineLocalization
        {
            DefinitionId = "TestStateMachineDefinitionId1",
            Item = "TestStateMachineDefinitionTransition1",
            Locale = "en-US",
            Value = "Transition1 EN",
        },
    ];
}
