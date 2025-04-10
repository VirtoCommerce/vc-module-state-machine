using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class SearchStateMachineLocalizationsQueryHandler : IQueryHandler<SearchStateMachineLocalizationsQuery, SearchStateMachineLocalizationResult>
{
    private readonly IStateMachineLocalizationSearchService _stateMachineLocalizationSearchService;

    public SearchStateMachineLocalizationsQueryHandler(IStateMachineLocalizationSearchService stateMachineLocalizationSearchService)
    {
        _stateMachineLocalizationSearchService = stateMachineLocalizationSearchService;
    }

    public virtual async Task<SearchStateMachineLocalizationResult> Handle(SearchStateMachineLocalizationsQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var searchCriteria = request.ToCriteria();
        var result = await _stateMachineLocalizationSearchService.SearchAsync(searchCriteria);
        return result;
    }

}
