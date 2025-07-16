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
public class GetStateMachineInstanceQueryHandler : IQueryHandler<GetStateMachineInstanceQuery, StateMachineInstance>
{
    private readonly IStateMachineInstanceService _stateMachineInstanceService;
    private readonly IStateMachineLocalizationSearchService _stateMachineLocalizationSearchService;
    private readonly IStateMachineAttributeSearchService _stateMachineAttributeSearchService;

    public GetStateMachineInstanceQueryHandler(
        IStateMachineInstanceService stateMachineInstanceService,
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService,
        IStateMachineAttributeSearchService stateMachineAttributeSearchService
        )
    {
        _stateMachineInstanceService = stateMachineInstanceService;
        _stateMachineLocalizationSearchService = stateMachineLocalizationSearchService;
        _stateMachineAttributeSearchService = stateMachineAttributeSearchService;
    }

    public virtual async Task<StateMachineInstance> Handle(GetStateMachineInstanceQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrEmpty(request.StateMachineInstanceId))
        {
            throw new ArgumentNullException(nameof(request.StateMachineInstanceId));
        }

        var result = await _stateMachineInstanceService.GetByIdAsync(request.StateMachineInstanceId);
        result.Evaluate(new StateMachineTriggerContext { Principal = request.User });

        if (result.StateMachineDefinition != null)
        {
            var locale = !string.IsNullOrEmpty(request.Locale) ? request.Locale : "en-US";

            var localizationSearchCriteria = new SearchStateMachineLocalizationCriteria { DefinitionId = result.StateMachineDefinitionId, Locale = locale };
            var localizationSearchResults = (await _stateMachineLocalizationSearchService.SearchAsync(localizationSearchCriteria, false)).Results;

            var attributeSearchCriteria = new SearchStateMachineAttributeCriteria { DefinitionId = result.StateMachineDefinitionId };
            var attributeSearchResults = (await _stateMachineAttributeSearchService.SearchAsync(attributeSearchCriteria, false)).Results;

            if (localizationSearchResults.Any() || attributeSearchResults.Any())
            {
                foreach (var definitionState in result.StateMachineDefinition.States)
                {
                    definitionState.LocalizedValue = localizationSearchResults.FirstOrDefault(x => x.Item == definitionState.Name)?.Value;
                    definitionState.Attributes = attributeSearchResults.Where(x => x.Item == definitionState.Name).ToList();
                    foreach (var definitionStateTransition in definitionState.Transitions)
                    {
                        definitionStateTransition.LocalizedValue = localizationSearchResults.FirstOrDefault(x => x.Item == definitionStateTransition.Trigger)?.Value;
                        definitionStateTransition.Attributes = attributeSearchResults.Where(x => x.Item == definitionStateTransition.Trigger).ToList();
                    }
                }
            }
        }

        return result;
    }
}
