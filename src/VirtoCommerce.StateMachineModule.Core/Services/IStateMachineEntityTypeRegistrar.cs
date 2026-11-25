using System.Collections.Generic;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Core.Services;

/// <summary>
/// Entity types state machines can be defined for. A module whose entities have state machines registers their type in
/// its <c>PostInitialize</c>; the admin UI lists the registered types. Replaces the legacy client-side
/// <c>stateMachineTypes.addType</c>, which the Admin Shell can't see.
/// </summary>
public interface IStateMachineEntityTypeRegistrar
{
    /// <summary>Registers an entity type; registering an id again replaces it.</summary>
    void Register(StateMachineEntityType entityType);

    /// <summary>The registered entity types, in registration order.</summary>
    IList<StateMachineEntityType> GetAll();

    /// <summary>A new condition tree prototype of the entity type; null for an unknown type or one without conditions.</summary>
    IConditionTree GetConditionTreePrototype(string entityTypeId);
}
