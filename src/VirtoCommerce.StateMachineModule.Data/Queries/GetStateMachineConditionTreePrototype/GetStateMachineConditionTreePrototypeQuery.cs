using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.StateMachineModule.Core.Common;

namespace VirtoCommerce.StateMachineModule.Data.Queries;

public class GetStateMachineConditionTreePrototypeQuery : IQuery<IConditionTree>
{
    public string EntityTypeId { get; set; }
}
