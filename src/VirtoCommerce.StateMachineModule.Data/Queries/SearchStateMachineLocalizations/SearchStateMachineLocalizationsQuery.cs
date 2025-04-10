using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models.Search;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class SearchStateMachineLocalizationsQuery : SearchStateMachineLocalizationCriteria, IQuery<SearchStateMachineLocalizationResult>
{
    public virtual SearchStateMachineLocalizationCriteria ToCriteria()
    {
        var criteria = new SearchStateMachineLocalizationCriteria();

        criteria.DefinitionId = DefinitionId;
        criteria.Item = Item;
        criteria.Locale = Locale;
        criteria.ObjectIds = ObjectIds;
        criteria.Take = Take;
        criteria.Skip = Skip;
        criteria.SearchPhrase = SearchPhrase;

        return criteria;
    }
}
