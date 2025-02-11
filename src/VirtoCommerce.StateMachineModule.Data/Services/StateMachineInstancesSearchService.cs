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
public class StateMachineInstancesSearchService : SearchService<SearchStateMachineInstancesCriteria, SearchStateMachineInstancesResult, StateMachineInstance, StateMachineInstanceEntity>,
    IStateMachineInstancesSearchService
{
    public StateMachineInstancesSearchService(
        Func<IStateMachineRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IStateMachineInstanceService crudService,
        IOptions<CrudOptions> crudOptions
        )
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }
    protected override IQueryable<StateMachineInstanceEntity> BuildQuery(IRepository repository, SearchStateMachineInstancesCriteria criteria)
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

    protected override IList<SortInfo> BuildSortExpression(SearchStateMachineInstancesCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;
        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos = new[]
            {
                    new SortInfo
                    {
                        SortColumn = ReflectionUtility.GetPropertyName<StateMachineInstanceEntity>(x => x.CreatedDate),
                        SortDirection = SortDirection.Descending
                    }
                };
        }

        return sortInfos;
    }
}
