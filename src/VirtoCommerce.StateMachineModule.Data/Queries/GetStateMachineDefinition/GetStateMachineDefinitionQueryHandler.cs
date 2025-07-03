using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class GetStateMachineDefinitionQueryHandler : IQueryHandler<GetStateMachineDefinitionQuery, StateMachineDefinition>
{
    private readonly IStateMachineDefinitionService _stateMachineDefinitionService;
    private readonly IStateMachineLocalizationSearchService _stateMachineLocalizationSearchService;
    private readonly IStateMachineAttributeSearchService _stateMachineAttributeSearchService;

    public GetStateMachineDefinitionQueryHandler(
        IStateMachineDefinitionService stateMachineDefinitionService,
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService,
        IStateMachineAttributeSearchService stateMachineAttributeSearchService
        )
    {
        _stateMachineDefinitionService = stateMachineDefinitionService;
        _stateMachineLocalizationSearchService = stateMachineLocalizationSearchService;
        _stateMachineAttributeSearchService = stateMachineAttributeSearchService;
    }

    public virtual async Task<StateMachineDefinition> Handle(GetStateMachineDefinitionQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrEmpty(request.StateMachineDefinitionId))
        {
            throw new ArgumentNullException(nameof(request.StateMachineDefinitionId));
        }

        var result = await _stateMachineDefinitionService.GetByIdAsync(request.StateMachineDefinitionId);

        if (result != null && !string.IsNullOrEmpty(request.Locale))
        {
            var localizationSearchCriteria = new SearchStateMachineLocalizationCriteria { DefinitionId = result.Id, Locale = request.Locale };
            var localizationSearchResults = (await _stateMachineLocalizationSearchService.SearchAsync(localizationSearchCriteria, false)).Results;

            var attributeSearchCriteria = new SearchStateMachineAttributeCriteria { DefinitionId = result.Id };
            var attributeSearchResults = (await _stateMachineAttributeSearchService.SearchAsync(attributeSearchCriteria, false)).Results;

            if (localizationSearchResults.Any() || attributeSearchResults.Any())
            {
                foreach (var state in result.States)
                {
                    state.LocalizedValue = localizationSearchResults.FirstOrDefault(x => x.Item == state.Name)?.Value;
                    state.Attributes = attributeSearchResults.Where(x => x.Item == state.Name).ToList();
                    foreach (var transition in state.Transitions)
                    {
                        transition.LocalizedValue = localizationSearchResults.FirstOrDefault(x => x.Item == transition.Trigger)?.Value;
                        transition.Attributes = attributeSearchResults.Where(x => x.Item == transition.Trigger).ToList();
                    }
                }
            }
        }

        return result;
    }
}
