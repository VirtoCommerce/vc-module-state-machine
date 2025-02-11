using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class SearchStateMachineDefinitionsQueryHandler : IQueryHandler<SearchStateMachineDefinitionsQuery, SearchStateMachineDefinitionsResult>
{
    private readonly IStateMachineDefinitionsSearchService _stateMachineDefinitionsSearchService;

    public SearchStateMachineDefinitionsQueryHandler(IStateMachineDefinitionsSearchService stateMachineDefinitionsSearchService)
    {
        _stateMachineDefinitionsSearchService = stateMachineDefinitionsSearchService;
    }

    public virtual async Task<SearchStateMachineDefinitionsResult> Handle(SearchStateMachineDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var searchCriteria = request.ToCriteria();
        var result = await _stateMachineDefinitionsSearchService.SearchAsync(searchCriteria);
        return result;
    }
}
