using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Queries;
public class SearchStateMachineDefinitionsQueryHandler : IQueryHandler<SearchStateMachineDefinitionsQuery, SearchStateMachineDefinitionResult>
{
    private readonly IStateMachineDefinitionSearchService _stateMachineDefinitionsSearchService;

    public SearchStateMachineDefinitionsQueryHandler(IStateMachineDefinitionSearchService stateMachineDefinitionsSearchService)
    {
        _stateMachineDefinitionsSearchService = stateMachineDefinitionsSearchService;
    }

    public virtual async Task<SearchStateMachineDefinitionResult> Handle(SearchStateMachineDefinitionsQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var searchCriteria = request.ToCriteria();
        var result = await _stateMachineDefinitionsSearchService.SearchAsync(searchCriteria);
        return result;
    }
}
