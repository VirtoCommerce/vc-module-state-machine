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
public class GetStateMachineDefinitionStatesQueryHandler : IQueryHandler<GetStateMachineDefinitionStatesQuery, StateMachineStateShort[]>
{
    private readonly IStateMachineDefinitionService _stateMachineDefinitionService;
    private readonly IStateMachineLocalizationSearchService _stateMachineLocalizationSearchService;

    public GetStateMachineDefinitionStatesQueryHandler(
        IStateMachineDefinitionService stateMachineDefinitionService,
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService
    )
    {
        _stateMachineDefinitionService = stateMachineDefinitionService;
        _stateMachineLocalizationSearchService = stateMachineLocalizationSearchService;
    }

    public virtual async Task<StateMachineStateShort[]> Handle(GetStateMachineDefinitionStatesQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrEmpty(request.EntityType))
        {
            throw new ArgumentNullException(nameof(request.EntityType));
        }

        var entityType = request.EntityType;

        var stateMachineDefinition = await _stateMachineDefinitionService.GetActiveStateMachineDefinitionAsync(entityType);
        if (stateMachineDefinition == null)
        {
            throw new InvalidOperationException($"State Machine Definition for type {entityType} not found");
        }

        if (stateMachineDefinition.States.IsNullOrEmpty())
        {
            throw new InvalidOperationException($"States for {entityType} not found");
        }

        if (!string.IsNullOrEmpty(request.Locale))
        {
            var localizationSearchCriteria = new SearchStateMachineLocalizationCriteria { DefinitionId = stateMachineDefinition.Id, Locale = request.Locale };
            var localizationSearchResults = (await _stateMachineLocalizationSearchService.SearchAsync(localizationSearchCriteria, false)).Results;
            if (localizationSearchResults.Any())
            {
                foreach (var state in stateMachineDefinition.States)
                {
                    state.LocalizedValue = localizationSearchResults.FirstOrDefault(x => x.Item == state.Name)?.Value;
                }
            }

        }

        var result = stateMachineDefinition.States.Select(x => new StateMachineStateShort(x)).ToArray();
        return result;
    }
}
