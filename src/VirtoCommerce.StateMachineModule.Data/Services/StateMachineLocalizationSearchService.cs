using System;
using System.Collections.Generic;
using System.Linq;
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
public class StateMachineLocalizationSearchService : SearchService<SearchStateMachineLocalizationCriteria, SearchStateMachineLocalizationResult, StateMachineLocalization, StateMachineLocalizationEntity>,
    IStateMachineLocalizationSearchService
{
    public StateMachineLocalizationSearchService(
        Func<IStateMachineRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IStateMachineLocalizationCrudService crudService,
        IOptions<CrudOptions> crudOptions
        )
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }

    protected override IQueryable<StateMachineLocalizationEntity> BuildQuery(IRepository repository, SearchStateMachineLocalizationCriteria criteria)
    {
        var query = ((IStateMachineRepository)repository).StateMachineLocalizations;

        if (!string.IsNullOrEmpty(criteria.DefinitionId))
        {
            query = query.Where(x => x.DefinitionId == criteria.DefinitionId);
        }

        if (!string.IsNullOrEmpty(criteria.Item))
        {
            query = query.Where(x => x.Item == criteria.Item);
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(SearchStateMachineLocalizationCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;
        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
                [
                    new SortInfo
                    {
                        SortColumn = ReflectionUtility.GetPropertyName<StateMachineLocalizationEntity>(x => x.Locale),
                        SortDirection = SortDirection.Ascending
                    }
                ];
        }

        return sortInfos;
    }
}
