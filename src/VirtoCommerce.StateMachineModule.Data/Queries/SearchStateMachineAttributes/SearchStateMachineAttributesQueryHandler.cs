using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class SearchStateMachineAttributesQueryHandler : IQueryHandler<SearchStateMachineAttributesQuery, SearchStateMachineAttributeResult>
{
    private readonly IStateMachineAttributeSearchService _stateMachineAttributeSearchService;

    public SearchStateMachineAttributesQueryHandler(IStateMachineAttributeSearchService stateMachineAttributeSearchService)
    {
        _stateMachineAttributeSearchService = stateMachineAttributeSearchService;
    }

    public virtual async Task<SearchStateMachineAttributeResult> Handle(SearchStateMachineAttributesQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var searchCriteria = request.ToCriteria();
        var result = await _stateMachineAttributeSearchService.SearchAsync(searchCriteria, false);
        return result;
    }

}
