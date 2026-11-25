using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;

public class GetStateMachineEntityTypesQueryHandler : IQueryHandler<GetStateMachineEntityTypesQuery, IList<StateMachineEntityType>>
{
    private readonly IStateMachineEntityTypeRegistrar _stateMachineEntityTypeRegistrar;

    public GetStateMachineEntityTypesQueryHandler(
        IStateMachineEntityTypeRegistrar stateMachineEntityTypeRegistrar
        )
    {
        _stateMachineEntityTypeRegistrar = stateMachineEntityTypeRegistrar;
    }

    public virtual Task<IList<StateMachineEntityType>> Handle(GetStateMachineEntityTypesQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return Task.FromResult(_stateMachineEntityTypeRegistrar.GetAll());
    }
}
