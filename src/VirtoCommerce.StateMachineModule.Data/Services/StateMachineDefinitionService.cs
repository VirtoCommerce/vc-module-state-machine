using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data.Models;
using VirtoCommerce.StateMachineModule.Data.Repositories;
using VirtoCommerce.StateMachineModule.Data.Validators;

namespace VirtoCommerce.StateMachineModule.Data.Services;
public class StateMachineDefinitionService : CrudService<StateMachineDefinition, StateMachineDefinitionEntity,
     GenericChangedEntryEvent<StateMachineDefinition>,
     GenericChangedEntryEvent<StateMachineDefinition>>, IStateMachineDefinitionService
{
    private readonly Func<IStateMachineRepository> _repositoryFactory;

    public StateMachineDefinitionService(
        Func<IStateMachineRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
        _repositoryFactory = repositoryFactory;
    }

    public virtual async Task<StateMachineDefinition> GetActiveStateMachineDefinitionAsync(string entityType)
    {
        using var repository = _repositoryFactory();

        var stateMachineDefinitionEntity = await repository.StateMachineDefinitions
            .FirstOrDefaultAsync(x => x.EntityType == entityType && x.IsActive);

        return stateMachineDefinitionEntity.ToModel(ExType<StateMachineDefinition>.New());
    }

    public virtual async Task<StateMachineDefinition> SaveStateMachineDefinitionAsync(StateMachineDefinition definition)
    {
        var validator = new StateMachineValidator();
        await validator.ValidateAndThrowAsync(definition);

        await SaveChangesAsync(new[] { definition });
        return definition;
    }

    protected override async Task<IList<StateMachineDefinitionEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return await ((IStateMachineRepository)repository).GetStateMachineDefinitionsByIds(ids.ToArray(), responseGroup);
    }
}
