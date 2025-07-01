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
public class StateMachineAttributeSearchService : SearchService<SearchStateMachineAttributeCriteria, SearchStateMachineAttributeResult, StateMachineAttribute, StateMachineAttributeEntity>,
    IStateMachineAttributeSearchService
{
    public StateMachineAttributeSearchService(
        Func<IStateMachineRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IStateMachineAttributeCrudService crudService,
        IOptions<CrudOptions> crudOptions
        )
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }

    protected override IQueryable<StateMachineAttributeEntity> BuildQuery(IRepository repository, SearchStateMachineAttributeCriteria criteria)
    {
        var query = ((IStateMachineRepository)repository).StateMachineAttributes;

        if (!criteria.DefinitionIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.DefinitionIds.Contains(x.DefinitionId));
        }
        else if (!string.IsNullOrEmpty(criteria.DefinitionId))
        {
            query = query.Where(x => x.DefinitionId == criteria.DefinitionId);
        }

        if (!string.IsNullOrEmpty(criteria.Item))
        {
            query = query.Where(x => x.Item == criteria.Item);
        }

        if (!string.IsNullOrEmpty(criteria.AttributeKey))
        {
            query = query.Where(x => x.AttributeKey.StartsWith(criteria.AttributeKey));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(SearchStateMachineAttributeCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;
        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
                [
                    new SortInfo
                    {
                        SortColumn = ReflectionUtility.GetPropertyName<StateMachineAttributeEntity>(x => x.AttributeKey),
                        SortDirection = SortDirection.Ascending
                    }
                ];
        }

        return sortInfos;
    }
}

