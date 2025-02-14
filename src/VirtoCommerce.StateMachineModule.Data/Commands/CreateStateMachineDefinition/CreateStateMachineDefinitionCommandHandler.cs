using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Commands;
public class CreateStateMachineDefinitionCommandHandler : ICommandHandler<CreateStateMachineDefinitionCommand, StateMachineDefinition>
{
    private readonly IStateMachineDefinitionService _stateMachineDefinitionService;

    public CreateStateMachineDefinitionCommandHandler(IStateMachineDefinitionService stateMachineDefinitionService)
    {
        _stateMachineDefinitionService = stateMachineDefinitionService;
    }

    public virtual async Task<StateMachineDefinition> Handle(CreateStateMachineDefinitionCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Definition == null)
        {
            throw new ArgumentNullException(nameof(request.Definition));
        }

        return await _stateMachineDefinitionService.SaveStateMachineDefinitionAsync(request.Definition);
    }
}
