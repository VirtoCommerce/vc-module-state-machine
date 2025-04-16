using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Search;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data.Models;
using VirtoCommerce.StateMachineModule.Data.Repositories;

namespace VirtoCommerce.StateMachineModule.Data.Services;
public class StateMachineDefinitionSearchService : SearchService<SearchStateMachineDefinitionCriteria, SearchStateMachineDefinitionResult, StateMachineDefinition, StateMachineDefinitionEntity>,
    IStateMachineDefinitionSearchService
{
    private readonly IStateMachineLocalizationSearchService _stateMachineLocalizationSearchService;

    public StateMachineDefinitionSearchService(
        Func<IStateMachineRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IStateMachineDefinitionService crudService,
        IOptions<CrudOptions> crudOptions,
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService
        )
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
        _stateMachineLocalizationSearchService = stateMachineLocalizationSearchService;
    }
    protected override IQueryable<StateMachineDefinitionEntity> BuildQuery(IRepository repository, SearchStateMachineDefinitionCriteria criteria)
    {
        var query = ((IStateMachineRepository)repository).StateMachineDefinitions;

        if (!criteria.ObjectTypes.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.ObjectTypes.Contains(x.EntityType));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(SearchStateMachineDefinitionCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;
        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
                [
                    new SortInfo
                    {
                        SortColumn = ReflectionUtility.GetPropertyName<StateMachineDefinitionEntity>(x => x.CreatedDate),
                        SortDirection = SortDirection.Descending
                    }
                ];
        }

        return sortInfos;
    }

    protected override async Task<SearchStateMachineDefinitionResult> ProcessSearchResultAsync(SearchStateMachineDefinitionResult result, SearchStateMachineDefinitionCriteria criteria)
    {
        var respGroupEnum = EnumUtility.SafeParseFlags(criteria.ResponseGroup, StateMachineResponseGroup.None);
        if (respGroupEnum.HasFlag(StateMachineResponseGroup.WithLocalization)
            && !string.IsNullOrEmpty(criteria.Locale))
        {
            if (!result.Results.IsNullOrEmpty())
            {
                var definitionIds = result.Results.Select(x => x.Id).ToArray();
                var localizationSearchCriteria = new SearchStateMachineLocalizationCriteria { DefinitionIds = definitionIds, Locale = criteria.Locale };
                var localizationSearchResults = (await _stateMachineLocalizationSearchService.SearchAsync(localizationSearchCriteria, false)).Results;
                foreach (var definition in result.Results)
                {
                    var definitionLocalizations = localizationSearchResults.Where(x => x.DefinitionId == definition.Id);
                    if (definitionLocalizations.Any())
                    {
                        foreach (var definitionState in definition.States)
                        {
                            definitionState.LocalizedValue = definitionLocalizations.FirstOrDefault(x => x.Item == definitionState.Name)?.Value;
                            foreach (var definitionStateTransition in definitionState.Transitions)
                            {
                                definitionStateTransition.LocalizedValue = definitionLocalizations.FirstOrDefault(x => x.Item == definitionStateTransition.Trigger)?.Value;
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
}
