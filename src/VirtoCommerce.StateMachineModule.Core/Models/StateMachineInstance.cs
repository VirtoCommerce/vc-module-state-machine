using System;
using System.Collections.Generic;
using System.Linq;
using Stateless;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StateMachineModule.Core.Models;
public class StateMachineInstance : AuditableEntity, ICloneable
{
    private StateMachine<string, string> _stateMachine;

    public string EntityId { get; set; }
    public string EntityType { get; set; }
    public string StateMachineDefinitionId => StateMachineDefinition?.Id;
    public string StateMachineName => StateMachineDefinition?.Name;
    public string CurrentStateName => _stateMachine?.State;
    public StateMachineState CurrentState => StateMachineDefinition?.States.FirstOrDefault(x => x.Name == _stateMachine.State);
    public IEnumerable<string> PermittedTriggers { get; set; }
    private IList<StateMachine<string, string>.TriggerWithParameters<StateMachineTriggerContext>> _registeredTriggersList = new List<StateMachine<string, string>.TriggerWithParameters<StateMachineTriggerContext>>();
    public bool IsActive => !CurrentState?.IsFinal ?? false;
    public StateMachineDefinition StateMachineDefinition { get; set; }

    public StateMachineInstance()
    {
    }

    public virtual StateMachineInstance Configure(StateMachineDefinition definition, string state)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }
        StateMachineDefinition = definition;
        StateMachineState currentState = null;

        var initialState = definition.States.FirstOrDefault(x => x.IsInitial);
        if (initialState == null)
        {
            throw new OperationCanceledException("initialState must be set");
        }
        var nullState = new StateMachineState
        {
            Name = "Null",
            Transitions =
            [
                new StateMachineTransition()
                {
                    ToState = initialState.Name,
                    Trigger = $"Start"
                }
            ]
        };
        if (!string.IsNullOrEmpty(state))
        {
            currentState = definition.States.FirstOrDefault(x => x.Name.EqualsInvariant(state));
        }
        if (currentState == null)
        {
            currentState = nullState;
        }
        _stateMachine = new StateMachine<string, string>(currentState.Name);

        foreach (var availState in definition.States.Concat([nullState]))
        {
            var configuration = _stateMachine.Configure(availState.Name);
            foreach (var permittedTransition in availState.Transitions ?? Array.Empty<StateMachineTransition>())
            {
                var trigger = _registeredTriggersList.FirstOrDefault(x => x.Trigger == permittedTransition.Trigger);
                if (trigger == null)
                {
                    trigger = _stateMachine.SetTriggerParameters<StateMachineTriggerContext>(permittedTransition.Trigger);
                    _registeredTriggersList.Add(trigger);
                }

                configuration.PermitIf(trigger, permittedTransition.ToState, (ctx) => true);
            }
        }

        return this;
    }

    public virtual StateMachineInstance Start(StateMachineTriggerContext context)
    {
        if (CurrentStateName == "Null")
        {
            return Fire("Start", context);
        }
        throw new OperationCanceledException("Already started");
    }

    public virtual StateMachineInstance Evaluate(StateMachineTriggerContext context)
    {
        PermittedTriggers = _stateMachine.GetPermittedTriggers(context);
        return this;
    }

    public virtual StateMachineInstance Fire(string trigger, StateMachineTriggerContext context)
    {
        var paramsTrigger = _registeredTriggersList.FirstOrDefault(x => x.Trigger == trigger);
        _stateMachine.Fire(paramsTrigger, context);
        Evaluate(context);
        return this;
    }

    public virtual object Clone()
    {
        return MemberwiseClone();
    }

}
