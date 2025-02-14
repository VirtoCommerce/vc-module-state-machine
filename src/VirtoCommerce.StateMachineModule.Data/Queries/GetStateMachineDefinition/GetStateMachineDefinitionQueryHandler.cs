using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class GetStateMachineDefinitionQueryHandler : IQueryHandler<GetStateMachineDefinitionQuery, StateMachineDefinition>
{
    private readonly IStateMachineDefinitionService _stateMachineDefinitionService;

    public GetStateMachineDefinitionQueryHandler(IStateMachineDefinitionService stateMachineDefinitionService)
    {
        _stateMachineDefinitionService = stateMachineDefinitionService;
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
        return result;
    }
}
