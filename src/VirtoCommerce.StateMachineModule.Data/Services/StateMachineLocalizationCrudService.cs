using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data.Models;
using VirtoCommerce.StateMachineModule.Data.Repositories;

namespace VirtoCommerce.StateMachineModule.Data.Services;
public class StateMachineLocalizationCrudService : CrudService<StateMachineLocalization, StateMachineLocalizationEntity,
    GenericChangedEntryEvent<StateMachineLocalization>, GenericChangedEntryEvent<StateMachineLocalization>>,
    IStateMachineLocalizationCrudService
{
    public StateMachineLocalizationCrudService(
        Func<IStateMachineRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher
        ) : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
    }
    protected override async Task<IList<StateMachineLocalizationEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return await ((IStateMachineRepository)repository).GetStateMachineLocalizationsByIds(ids.ToArray(), responseGroup);
    }
}
