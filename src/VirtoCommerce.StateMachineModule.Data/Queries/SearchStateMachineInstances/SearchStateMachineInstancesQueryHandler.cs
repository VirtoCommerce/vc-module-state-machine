using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class SearchStateMachineInstancesQueryHandler : IQueryHandler<SearchStateMachineInstancesQuery, SearchStateMachineInstancesResult>
{
    private readonly IStateMachineInstancesSearchService _stateMachineInstancesSearchService;

    public SearchStateMachineInstancesQueryHandler(IStateMachineInstancesSearchService stateMachineInstancesSearchService)
    {
        _stateMachineInstancesSearchService = stateMachineInstancesSearchService;
    }

    public virtual async Task<SearchStateMachineInstancesResult> Handle(SearchStateMachineInstancesQuery request, CancellationToken cancellationToken)
    {
        var searchCriteria = request.ToCriteria();
        var result = await _stateMachineInstancesSearchService.SearchAsync(searchCriteria);
        return result;
    }
}
