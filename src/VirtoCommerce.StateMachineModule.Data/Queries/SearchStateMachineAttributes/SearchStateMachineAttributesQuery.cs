using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models.Search;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class SearchStateMachineAttributesQuery : SearchStateMachineAttributeCriteria, IQuery<SearchStateMachineAttributeResult>
{
    public virtual SearchStateMachineAttributeCriteria ToCriteria()
    {
        var criteria = new SearchStateMachineAttributeCriteria();

        criteria.DefinitionId = DefinitionId;
        criteria.DefinitionIds = DefinitionIds;
        criteria.Item = Item;
        criteria.AttributeKey = AttributeKey;
        criteria.ObjectIds = ObjectIds;
        criteria.Take = Take;
        criteria.Skip = Skip;
        criteria.SearchPhrase = SearchPhrase;
        criteria.Sort = Sort;

        return criteria;
    }
}
