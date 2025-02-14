using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Events;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data.Models;
using VirtoCommerce.StateMachineModule.Data.Repositories;

namespace VirtoCommerce.StateMachineModule.Data.Services;
public class StateMachineInstanceService : CrudService<StateMachineInstance, StateMachineInstanceEntity,
     GenericChangedEntryEvent<StateMachineInstance>,
     GenericChangedEntryEvent<StateMachineInstance>>, IStateMachineInstanceService
{
    private readonly Func<IStateMachineRepository> _repositoryFactory;
    private readonly IEventPublisher _eventPublisher;
    private readonly IStateMachineDefinitionService _stateMachineDefinitionService;

    public StateMachineInstanceService(
        Func<IStateMachineRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher,
        IStateMachineDefinitionService stateMachineDefinitionService)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
        _repositoryFactory = repositoryFactory;
        _eventPublisher = eventPublisher;
        _stateMachineDefinitionService = stateMachineDefinitionService;
    }

    public virtual async Task<StateMachineInstance> CreateStateMachineInstanceAsync(string stateMachineDefinitionId, string stateMachineInstanceId, IHasDynamicProperties entity)
    {
        var stateMachineDefinition = await _stateMachineDefinitionService.GetByIdAsync(stateMachineDefinitionId);
        if (stateMachineDefinition == null)
        {
            throw new OperationCanceledException($"SM with {stateMachineDefinitionId} not found");
        }

        var stateMachineInstance = new StateMachineInstance().Configure(stateMachineDefinition, null);
        stateMachineInstance.Id = stateMachineInstanceId.EmptyToNull() ?? Guid.NewGuid().ToString();
        stateMachineInstance.EntityId = entity.Id;
        stateMachineInstance.EntityType = stateMachineDefinition.EntityType;

        var context = new StateMachineTriggerContext { ContextObject = entity };
        stateMachineInstance.Evaluate(context);
        stateMachineInstance.Start(context);

        await SaveChangesAsync([stateMachineInstance]);

        return stateMachineInstance;
    }

    public virtual async Task<StateMachineInstance> FireTriggerAsync(StateMachineInstance stateMachineInstance, string trigger, StateMachineTriggerContext context)
    {
        var firedInstance = stateMachineInstance.Fire(trigger, context);

        var triggerEvent = ExType<StateMachineTriggerEvent>.New();
        triggerEvent.EntityId = firedInstance.EntityId;
        triggerEvent.EntityType = firedInstance.EntityType;
        triggerEvent.StateName = firedInstance.CurrentStateName;
        await _eventPublisher.Publish(triggerEvent);

        return firedInstance;
    }

    public virtual async Task<IList<StateMachineInstance>> GetForEntity(string entityId, string entityType)
    {
        using var repository = _repositoryFactory();

        var ids = await repository.StateMachineInstances
            .Where(x => x.EntityId == entityId && x.EntityType == entityType)
            .Select(x => x.Id)
            .ToListAsync();

        return await GetAsync(ids);
    }

    protected override async Task<IList<StateMachineInstanceEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return await ((IStateMachineRepository)repository).GetStateMachineInstancesByIds(ids.ToArray(), responseGroup);
    }
}
