using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Commands;
public class FireStateMachineTriggerCommandHandler : ICommandHandler<FireStateMachineTriggerCommand, StateMachineInstance>
{
    private readonly IStateMachineInstanceService _stateMachineInstanceService;
    private readonly IStateMachineLocalizationSearchService _stateMachineLocalizationSearchService;
    private readonly IStateMachineAttributeSearchService _stateMachineAttributeSearchService;

    public FireStateMachineTriggerCommandHandler(
        IStateMachineInstanceService stateMachineInstanceService,
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService,
        IStateMachineAttributeSearchService stateMachineAttributeSearchService
        )
    {
        _stateMachineInstanceService = stateMachineInstanceService;
        _stateMachineLocalizationSearchService = stateMachineLocalizationSearchService;
        _stateMachineAttributeSearchService = stateMachineAttributeSearchService;
    }

    public virtual async Task<StateMachineInstance> Handle(FireStateMachineTriggerCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrEmpty(request.StateMachineInstanceId))
        {
            throw new ArgumentNullException(nameof(request.StateMachineInstanceId));
        }

        if (string.IsNullOrEmpty(request.Trigger))
        {
            throw new ArgumentNullException(nameof(request.Trigger));
        }

        if (string.IsNullOrEmpty(request.EntityId))
        {
            throw new ArgumentNullException(nameof(request.EntityId));
        }

        var instanceId = request.StateMachineInstanceId;

        var instance = await _stateMachineInstanceService.GetByIdAsync(instanceId);
        if (instance == null)
        {
            throw new OperationCanceledException($"State machine instance with {instanceId} not found");
        }

        if (instance.StateMachineDefinition != null)
        {
            var locale = !string.IsNullOrEmpty(request.Locale) ? request.Locale : "en-US";

            var localizationSearchCriteria = new SearchStateMachineLocalizationCriteria { DefinitionId = instance.StateMachineDefinitionId, Locale = locale };
            var localizationSearchResults = (await _stateMachineLocalizationSearchService.SearchAsync(localizationSearchCriteria, false)).Results;

            var attributeSearchCriteria = new SearchStateMachineAttributeCriteria { DefinitionId = instance.StateMachineDefinitionId };
            var attributeSearchResults = (await _stateMachineAttributeSearchService.SearchAsync(attributeSearchCriteria, false)).Results;

            if (localizationSearchResults.Any() || attributeSearchResults.Any())
            {
                foreach (var definitionState in instance.StateMachineDefinition.States)
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

        var result = await _stateMachineInstanceService.FireTriggerAsync(instance, request.Trigger, new StateMachineTriggerContext { EntityId = request.EntityId, Principal = request.User });
        await _stateMachineInstanceService.SaveChangesAsync([result]);
        return result;
    }
}

