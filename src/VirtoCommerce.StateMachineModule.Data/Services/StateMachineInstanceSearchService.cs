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
public class StateMachineInstanceSearchService : SearchService<SearchStateMachineInstanceCriteria, SearchStateMachineInstanceResult, StateMachineInstance, StateMachineInstanceEntity>,
    IStateMachineInstanceSearchService
{
    private readonly IStateMachineLocalizationSearchService _stateMachineLocalizationSearchService;
    private readonly IStateMachineAttributeSearchService _stateMachineAttributeSearchService;

    public StateMachineInstanceSearchService(
        Func<IStateMachineRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IStateMachineInstanceService crudService,
        IOptions<CrudOptions> crudOptions,
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService,
        IStateMachineAttributeSearchService stateMachineAttributeSearchService
        )
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
        _stateMachineLocalizationSearchService = stateMachineLocalizationSearchService;
        _stateMachineAttributeSearchService = stateMachineAttributeSearchService;
    }

    protected override IQueryable<StateMachineInstanceEntity> BuildQuery(IRepository repository, SearchStateMachineInstanceCriteria criteria)
    {
        var query = ((IStateMachineRepository)repository).StateMachineInstances;

        if (!criteria.ObjectType.IsNullOrEmpty())
        {
            query = query.Where(x => x.EntityType == criteria.ObjectType);
        }

        if (!criteria.ObjectIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.ObjectIds.Contains(x.EntityId));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(SearchStateMachineInstanceCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;
        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
                [
                    new SortInfo
                    {
                        SortColumn = ReflectionUtility.GetPropertyName<StateMachineInstanceEntity>(x => x.CreatedDate),
                        SortDirection = SortDirection.Descending
                    }
                ];
        }

        return sortInfos;
    }

    protected override async Task<SearchStateMachineInstanceResult> ProcessSearchResultAsync(SearchStateMachineInstanceResult result, SearchStateMachineInstanceCriteria criteria)
    {
        var respGroupEnum = EnumUtility.SafeParseFlags(criteria.ResponseGroup, StateMachineResponseGroup.None);
        if (respGroupEnum.HasFlag(StateMachineResponseGroup.WithLocalization)
            && !string.IsNullOrEmpty(criteria.Locale))
        {
            if (!result.Results.IsNullOrEmpty())
            {
                var definitionIds = result.Results.Select(x => x.StateMachineDefinitionId).ToArray();

                var localizationSearchCriteria = new SearchStateMachineLocalizationCriteria { DefinitionIds = definitionIds, Locale = criteria.Locale };
                var localizationSearchResults = (await _stateMachineLocalizationSearchService.SearchAsync(localizationSearchCriteria, false)).Results;

                var attributeSearchCriteria = new SearchStateMachineAttributeCriteria { DefinitionIds = definitionIds };
                var attributeSearchResults = (await _stateMachineAttributeSearchService.SearchAsync(attributeSearchCriteria, false)).Results;

                foreach (var instance in result.Results)
                {
                    if (instance.StateMachineDefinition != null)
                    {
                        if (localizationSearchResults.Any() || attributeSearchResults.Any())
                        {
                            var definitionLocalizations = localizationSearchResults.Where(x => x.DefinitionId == instance.StateMachineDefinitionId);
                            var definitionAttributes = attributeSearchResults.Where(x => x.DefinitionId == instance.StateMachineDefinitionId);

                            foreach (var definitionState in instance.StateMachineDefinition.States)
                            {
                                definitionState.LocalizedValue = definitionLocalizations.FirstOrDefault(x => x.Item == definitionState.Name)?.Value;
                                definitionState.Attributes = definitionAttributes.Where(x => x.Item == definitionState.Name).ToList();
                                foreach (var definitionStateTransition in definitionState.Transitions)
                                {
                                    definitionStateTransition.LocalizedValue = definitionLocalizations.FirstOrDefault(x => x.Item == definitionStateTransition.Trigger)?.Value;
                                    definitionStateTransition.Attributes = definitionAttributes.Where(x => x.Item == definitionStateTransition.Trigger).ToList();
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
}
