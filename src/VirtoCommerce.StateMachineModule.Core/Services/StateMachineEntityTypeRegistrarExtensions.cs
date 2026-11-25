using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Core.Services;

public static class StateMachineEntityTypeRegistrarExtensions
{
    /// <summary>Registers an entity type by its id (full type name), without transition conditions.</summary>
    public static void Register(this IStateMachineEntityTypeRegistrar registrar, string id, string name)
    {
        registrar.Register(new StateMachineEntityType { Id = id, Name = name });
    }

    /// <summary>Registers <typeparamref name="T"/> as an entity type; its id is the type's full name.</summary>
    public static void Register<T>(this IStateMachineEntityTypeRegistrar registrar, string name)
    {
        registrar.Register(typeof(T).FullName, name);
    }

    /// <summary>
    /// Registers <typeparamref name="T"/> with transition conditions built from <typeparamref name="TPrototype"/>
    /// (created through <see cref="AbstractTypeFactory{T}"/>, so an overridden prototype is used).
    /// </summary>
    public static void Register<T, TPrototype>(this IStateMachineEntityTypeRegistrar registrar, string name)
        where TPrototype : IConditionTree
    {
        registrar.Register(new StateMachineEntityType
        {
            Id = typeof(T).FullName,
            Name = name,
            ConditionTreePrototypeFactory = () => AbstractTypeFactory<TPrototype>.TryCreateInstance(),
        });
    }
}
