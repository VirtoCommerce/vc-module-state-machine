using System;

namespace VirtoCommerce.StateMachineModule.Core.Models;
public class StateMachineStateShort
{
    public StateMachineStateShort(StateMachineState state)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        Name = state.Name;
        Description = state.Description;
        IsInitial = state.IsInitial;
        IsFinal = state.IsFinal;
        IsSuccess = state.IsSuccess;
        IsFailed = state.IsFailed;
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsInitial { get; set; }
    public bool IsFinal { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsFailed { get; set; }
}
