namespace VirtoCommerce.StateMachineModule.Core.Models;
public class StateMachineStateShort
{
    public StateMachineStateShort(StateMachineState state)
    {
        Name = state.Name;
        Description = state.Description;
        IsInitial = state.IsInitial;
        IsFinal = state.IsFinal;
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsInitial { get; set; }
    public bool IsFinal { get; set; }
}
