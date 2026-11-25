using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;

public class GetStateMachineConditionTreePrototypeQueryHandler : IQueryHandler<GetStateMachineConditionTreePrototypeQuery, IConditionTree>
{
    private readonly IStateMachineEntityTypeRegistrar _stateMachineEntityTypeRegistrar;

    public GetStateMachineConditionTreePrototypeQueryHandler(
        IStateMachineEntityTypeRegistrar stateMachineEntityTypeRegistrar
        )
    {
        _stateMachineEntityTypeRegistrar = stateMachineEntityTypeRegistrar;
    }

    public virtual Task<IConditionTree> Handle(GetStateMachineConditionTreePrototypeQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return Task.FromResult(_stateMachineEntityTypeRegistrar.GetConditionTreePrototype(request.EntityTypeId));
    }
}
