using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Data.Services;

/// <summary>In-memory registry of entity types; registered as a singleton and filled by modules at start-up.</summary>
public class StateMachineEntityTypeRegistrar : IStateMachineEntityTypeRegistrar
{
    private readonly List<StateMachineEntityType> _entityTypes = [];
    private readonly Lock _lock = new();

    public virtual void Register(StateMachineEntityType entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType.Id, nameof(entityType.Id));
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType.Name, nameof(entityType.Name));

        // a copy, so the caller can't change a registered type afterwards
        var registered = Copy(entityType);

        lock (_lock)
        {
            var index = _entityTypes.FindIndex(x => x.Id.EqualsIgnoreCase(registered.Id));
            if (index >= 0)
            {
                _entityTypes[index] = registered;
            }
            else
            {
                _entityTypes.Add(registered);
            }
        }
    }

    public virtual IList<StateMachineEntityType> GetAll()
    {
        lock (_lock)
        {
            return _entityTypes.Select(Copy).ToList();
        }
    }

    public virtual IConditionTree GetConditionTreePrototype(string entityTypeId)
    {
        Func<IConditionTree> factory;
        lock (_lock)
        {
            factory = _entityTypes.FirstOrDefault(x => x.Id.EqualsIgnoreCase(entityTypeId))?.ConditionTreePrototypeFactory;
        }

        // called outside the lock: the factory is the registering module's code
        return factory?.Invoke();
    }

    private static StateMachineEntityType Copy(StateMachineEntityType entityType)
    {
        return new StateMachineEntityType
        {
            Id = entityType.Id,
            Name = entityType.Name,
            ConditionTreePrototypeFactory = entityType.ConditionTreePrototypeFactory,
        };
    }
}
