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
    private readonly IStateMachineAttributeSearchService _stateMachineAttributeSearchService;

    public GetStateMachineInstanceForEntityQueryHandler(
        IStateMachineInstanceService stateMachineInstanceService,
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService,
        IStateMachineAttributeSearchService stateMachineAttributeSearchService
        )
    {
        _stateMachineInstanceService = stateMachineInstanceService;
        _stateMachineLocalizationSearchService = stateMachineLocalizationSearchService;
        _stateMachineAttributeSearchService = stateMachineAttributeSearchService;
    }

    public virtual async Task<StateMachineInstance> Handle(GetStateMachineInstanceForEntityQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var result = await _stateMachineInstanceService.GetForEntity(request.EntityId, request.EntityType);

        if (result != null)
        {
            if (!string.IsNullOrEmpty(request.Locale))
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

            var attributeSearchCriteria = new SearchStateMachineAttributeCriteria { DefinitionId = result.StateMachineDefinitionId };
            var attributeSearchResults = (await _stateMachineAttributeSearchService.SearchAsync(attributeSearchCriteria, false)).Results;
            if (attributeSearchResults.Any())
            {
                var currentState = result.CurrentState;
                currentState.Attributes = attributeSearchResults.Where(x => x.Item == currentState.Name).ToList();
                foreach (var transition in currentState.Transitions)
                {
                    transition.Attributes = attributeSearchResults.Where(x => x.Item == transition.Trigger).ToList();
                }
            }

            result.Evaluate(new StateMachineTriggerContext { Principal = request.User });
        }

        return result;
    }
}
