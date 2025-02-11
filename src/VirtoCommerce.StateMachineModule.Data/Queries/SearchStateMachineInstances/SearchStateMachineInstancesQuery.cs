using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models.Search;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class SearchStateMachineInstancesQuery : SearchStateMachineInstancesCriteria, IQuery<SearchStateMachineInstancesResult>
{
    public virtual SearchStateMachineInstancesCriteria ToCriteria()
    {
        var criteria = new SearchStateMachineInstancesCriteria();

        criteria.ObjectIds = ObjectIds;
        criteria.Take = Take;
        criteria.Skip = Skip;
        criteria.SearchPhrase = SearchPhrase;

        return criteria;
    }
}
