using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class GetStateMachineDefinitionStatesQueryHandler : IQueryHandler<GetStateMachineDefinitionStatesQuery, StateMachineStateShort[]>
{
    private readonly IStateMachineDefinitionService _stateMachineDefinitionService;

    public GetStateMachineDefinitionStatesQueryHandler(
        IStateMachineDefinitionService stateMachineDefinitionService
        )
    {
        _stateMachineDefinitionService = stateMachineDefinitionService;
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

        var states = stateMachineDefinition.States.Select(x => new StateMachineStateShort(x)).ToArray();
        return states;
    }
}
