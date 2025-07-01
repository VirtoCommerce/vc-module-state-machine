using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StateMachineModule.Core.Models.Search;
public class SearchStateMachineAttributeCriteria : SearchCriteriaBase
{
    public string DefinitionId { get; set; }
    public string[] DefinitionIds { get; set; }
    public string Item { get; set; }
    public string AttributeKey { get; set; }
}
