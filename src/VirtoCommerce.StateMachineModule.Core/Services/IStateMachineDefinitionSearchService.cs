using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;

namespace VirtoCommerce.StateMachineModule.Core.Services;
public interface IStateMachineDefinitionSearchService : ISearchService<SearchStateMachineDefinitionCriteria, SearchStateMachineDefinitionResult, StateMachineDefinition>
{
}
