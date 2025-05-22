using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class GetStateMachineInstanceForEntityQueryHandler : IQueryHandler<GetStateMachineInstanceForEntityQuery, StateMachineInstance>
{
    private readonly IStateMachineInstanceService _stateMachineInstanceService;
    private readonly IStateMachineLocalizationSearchService _stateMachineLocalizationSearchService;

    public GetStateMachineInstanceForEntityQueryHandler(
        IStateMachineInstanceService stateMachineInstanceService,
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService
        )
    {
        _stateMachineInstanceService = stateMachineInstanceService;
        _stateMachineLocalizationSearchService = stateMachineLocalizationSearchService;
    }

    public virtual async Task<StateMachineInstance> Handle(GetStateMachineInstanceForEntityQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var result = await _stateMachineInstanceService.GetForEntity(request.EntityId, request.EntityType);

        if (result != null && !string.IsNullOrEmpty(request.Locale))
        {
            var localizationSearchCriteria = new SearchStateMachineLocalizationCriteria { DefinitionId = result.StateMachineDefinitionId, Locale = request.Locale };
            var localizationSearchResults = (await _stateMachineLocalizationSearchService.SearchAsync(localizationSearchCriteria, false)).Results;
            if (localizationSearchResults.Any())
            {
                var currentState = result.CurrentState;
                currentState.LocalizedValue = localizationSearchResults.FirstOrDefault(x => x.Item == currentState.Name)?.Value;
                foreach (var transition in currentState.Transitions)
                {
                    transition.LocalizedValue = localizationSearchResults.FirstOrDefault(x => x.Item == transition.Trigger)?.Value;
                }
            }

        }

        result.Evaluate(new StateMachineTriggerContext { Principal = request.User });
        return result;
    }
}
