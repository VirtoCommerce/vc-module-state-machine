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

    public GetStateMachineDefinitionQueryHandler(
        IStateMachineDefinitionService stateMachineDefinitionService,
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService
        )
    {
        _stateMachineDefinitionService = stateMachineDefinitionService;
        _stateMachineLocalizationSearchService = stateMachineLocalizationSearchService;
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
            if (localizationSearchResults.Any())
            {
                foreach (var state in result.States)
                {
                    state.LocalizedValue = localizationSearchResults.FirstOrDefault(x => x.Item == state.Name)?.Value;
                    foreach (var transition in state.Transitions)
                    {
                        transition.LocalizedValue = localizationSearchResults.FirstOrDefault(x => x.Item == transition.Trigger)?.Value;
                    }
                }
            }

        }

        return result;
    }
}
