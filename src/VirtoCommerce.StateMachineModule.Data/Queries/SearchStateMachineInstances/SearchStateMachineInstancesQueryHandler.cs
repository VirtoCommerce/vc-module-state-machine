using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class SearchStateMachineInstancesQueryHandler : IQueryHandler<SearchStateMachineInstancesQuery, SearchStateMachineInstanceResult>
{
    private readonly IStateMachineInstanceSearchService _stateMachineInstancesSearchService;

    public SearchStateMachineInstancesQueryHandler(IStateMachineInstanceSearchService stateMachineInstancesSearchService)
    {
        _stateMachineInstancesSearchService = stateMachineInstancesSearchService;
    }

    public virtual async Task<SearchStateMachineInstanceResult> Handle(SearchStateMachineInstancesQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var searchCriteria = request.ToCriteria();
        var result = await _stateMachineInstancesSearchService.SearchAsync(searchCriteria);
        return result;
    }
}
