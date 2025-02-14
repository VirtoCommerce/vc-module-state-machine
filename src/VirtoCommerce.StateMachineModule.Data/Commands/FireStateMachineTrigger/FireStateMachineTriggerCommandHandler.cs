using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Commands;
public class FireStateMachineTriggerCommandHandler : ICommandHandler<FireStateMachineTriggerCommand, StateMachineInstance>
{
    private readonly IStateMachineInstanceService _stateMachineInstanceService;

    public FireStateMachineTriggerCommandHandler(IStateMachineInstanceService stateMachineInstanceService)
    {
        _stateMachineInstanceService = stateMachineInstanceService;
    }

    public virtual async Task<StateMachineInstance> Handle(FireStateMachineTriggerCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.StateMachineInstanceId == null)
        {
            throw new ArgumentNullException(nameof(request.StateMachineInstanceId));
        }

        if (request.Trigger == null)
        {
            throw new ArgumentNullException(nameof(request.Trigger));
        }

        if (request.EntityId == null)
        {
            throw new ArgumentNullException(nameof(request.EntityId));
        }

        var instanceId = request.StateMachineInstanceId;

        var instance = await _stateMachineInstanceService.GetByIdAsync(instanceId);
        if (instance == null)
        {
            throw new OperationCanceledException($"SM instance with {instanceId} not found");
        }
        var result = await _stateMachineInstanceService.FireTriggerAsync(instance, request.Trigger, new StateMachineTriggerContext { EntityId = request.EntityId });
        await _stateMachineInstanceService.SaveChangesAsync([result]);
        return result;
    }
}

