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
public class StateMachineDefinitionsSearchService : SearchService<SearchStateMachineDefinitionsCriteria, SearchStateMachineDefinitionsResult, StateMachineDefinition, StateMachineDefinitionEntity>,
    IStateMachineDefinitionsSearchService
{
    public StateMachineDefinitionsSearchService(
        Func<IStateMachineRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IStateMachineDefinitionService crudService,
        IOptions<CrudOptions> crudOptions
        )
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }
    protected override IQueryable<StateMachineDefinitionEntity> BuildQuery(IRepository repository, SearchStateMachineDefinitionsCriteria criteria)
    {
        var query = ((IStateMachineRepository)repository).StateMachineDefinitions;

        if (!criteria.ObjectTypes.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.ObjectTypes.Contains(x.EntityType));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(SearchStateMachineDefinitionsCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;
        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos = new[]
            {
                    new SortInfo
                    {
                        SortColumn = ReflectionUtility.GetPropertyName<StateMachineDefinitionEntity>(x => x.CreatedDate),
                        SortDirection = SortDirection.Descending
                    }
                };
        }

        return sortInfos;
    }
}
