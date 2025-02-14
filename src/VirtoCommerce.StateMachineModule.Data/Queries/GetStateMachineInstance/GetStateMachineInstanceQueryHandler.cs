using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class GetStateMachineInstanceQueryHandler : IQueryHandler<GetStateMachineInstanceQuery, StateMachineInstance>
{
    private readonly IStateMachineInstanceService _stateMachineInstanceService;

    public GetStateMachineInstanceQueryHandler(IStateMachineInstanceService stateMachineInstanceService)
    {
        _stateMachineInstanceService = stateMachineInstanceService;
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
        return result;
    }
}
