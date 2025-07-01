using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;

namespace VirtoCommerce.StateMachineModule.Core.Services;
public interface IStateMachineAttributeSearchService : ISearchService<SearchStateMachineAttributeCriteria, SearchStateMachineAttributeResult, StateMachineAttribute>
{
}
