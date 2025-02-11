using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.StateMachineModule.Core.Events;
public class StateMachineTriggerEvent : DomainEvent
{
    public StateMachineTriggerEvent()
    {
    }

    public string EntityId { get; set; }
    public string EntityType { get; set; }
    public string StateName { get; set; }
}
