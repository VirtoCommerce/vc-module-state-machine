using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models.Search;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class SearchStateMachineInstancesQuery : SearchStateMachineInstanceCriteria, IQuery<SearchStateMachineInstanceResult>
{
    public virtual SearchStateMachineInstanceCriteria ToCriteria()
    {
        var criteria = new SearchStateMachineInstanceCriteria();

        criteria.Locale = Locale;
        criteria.ResponseGroup = ResponseGroup;
        criteria.ObjectIds = ObjectIds;
        criteria.Take = Take;
        criteria.Skip = Skip;
        criteria.SearchPhrase = SearchPhrase;

        return criteria;
    }
}
