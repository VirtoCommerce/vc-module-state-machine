using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VirtoCommerce.StateMachineModule.Core.Models;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;
[ExcludeFromCodeCoverage]
public class StateMachineStateShortTests
{
    [Fact]
    public void Create_NullParam_ThrowsArgumentNullException()
    {
        // Arrange
        StateMachineState stateMachineState = null;

        // Act
        Action actual = () => new StateMachineStateShort(stateMachineState);

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Fact]
    public void Create_ValidParam_ReturnsActualValue()
    {
        // Arrange
        var stateMachineState = GetStateMachineState();

        // Act
        var stateMachineStateShort = new StateMachineStateShort(stateMachineState);

        // Assertion
        Assert.Equal(stateMachineStateShort.Name, stateMachineState.Name);
        Assert.Equal(stateMachineStateShort.Description, stateMachineState.Description);
        Assert.Equal(stateMachineStateShort.IsInitial, stateMachineState.IsInitial);
        Assert.Equal(stateMachineStateShort.IsFinal, stateMachineState.IsFinal);
        Assert.Equal(stateMachineStateShort.IsSuccess, stateMachineState.IsSuccess);
        Assert.Equal(stateMachineStateShort.IsFailed, stateMachineState.IsFailed);
    }

    private StateMachineState GetStateMachineState()
    {
        return new StateMachineState
        {
            Name = "TestStateName",
            Description = "Test state description",
            IsInitial = true,
            IsFinal = true,
            IsSuccess = true,
            IsFailed = true,
            StateData = null,
            Transitions = new List<StateMachineTransition>
            {
                new StateMachineTransition
                {
                    Trigger = "TestTrigger",
                    Description = "Test transition description",
                    ToState = "AnotherState"
                }
            }
        };
    }
}
