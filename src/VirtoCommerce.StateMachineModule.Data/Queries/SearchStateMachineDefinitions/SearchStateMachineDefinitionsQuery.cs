using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models.Search;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class SearchStateMachineDefinitionsQuery : SearchStateMachineDefinitionCriteria, IQuery<SearchStateMachineDefinitionResult>
{
    public virtual SearchStateMachineDefinitionCriteria ToCriteria()
    {
        var criteria = new SearchStateMachineDefinitionCriteria();

        criteria.Locale = Locale;
        criteria.ResponseGroup = ResponseGroup;
        criteria.ObjectIds = ObjectIds;
        criteria.Take = Take;
        criteria.Skip = Skip;
        criteria.SearchPhrase = SearchPhrase;

        return criteria;
    }
}
