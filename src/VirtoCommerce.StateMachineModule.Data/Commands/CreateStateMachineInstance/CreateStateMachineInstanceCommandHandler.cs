using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Commands;
public class CreateStateMachineInstanceCommandHandler : ICommandHandler<CreateStateMachineInstanceCommand, StateMachineInstance>
{
    private readonly IStateMachineInstanceService _stateMachineInstanceService;

    public CreateStateMachineInstanceCommandHandler(IStateMachineInstanceService stateMachineInstanceService)
    {
        _stateMachineInstanceService = stateMachineInstanceService;
    }

    public virtual async Task<StateMachineInstance> Handle(CreateStateMachineInstanceCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrEmpty(request.StateMachineDefinitionId))
        {
            throw new ArgumentNullException(nameof(request.StateMachineDefinitionId));
        }

        //if (request.Entity == null)
        //{
        //    throw new ArgumentNullException(nameof(request.Entity));
        //}

        return await _stateMachineInstanceService.CreateStateMachineInstanceAsync(request.StateMachineDefinitionId, request.StateMachineInstanceId, request.Entity);
    }
}
