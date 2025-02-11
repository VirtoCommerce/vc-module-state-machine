using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models.Search;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class SearchStateMachineDefinitionsQuery : SearchStateMachineDefinitionsCriteria, IQuery<SearchStateMachineDefinitionsResult>
{
    public virtual SearchStateMachineDefinitionsCriteria ToCriteria()
    {
        var criteria = new SearchStateMachineDefinitionsCriteria();

        criteria.Take = Take;
        criteria.Skip = Skip;
        criteria.SearchPhrase = SearchPhrase;

        return criteria;
    }
}
