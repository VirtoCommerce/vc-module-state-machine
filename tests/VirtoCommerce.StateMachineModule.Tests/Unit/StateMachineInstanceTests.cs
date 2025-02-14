using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VirtoCommerce.StateMachineModule.Core.Models;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;
[ExcludeFromCodeCoverage]
public class StateMachineInstanceTests
{
    [Fact]
    public void Configure_NullDefinition_ThrowsArgumentNullException()
    {
        // Arrange
        var stateMachineInstance = new StateMachineInstance();

        // Act
        Action actual = () => stateMachineInstance.Configure(null, "AnyState");

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Fact]
    public void Configure_BrokenDefinition_ThrowsOperationCanceledException()
    {
        // Arrange
        var stateMachineInstance = new StateMachineInstance();
        var stateMachineDefinitionBroken = GetStateMachineDefinitionBroken();

        // Act
        Action actual = () => stateMachineInstance.Configure(stateMachineDefinitionBroken, "AnyState");

        // Assertion
        Assert.Throws<OperationCanceledException>(actual);
    }

    [Fact]
    public void Configure_GoodDefinition_ReturnsActualValue()
    {
        // Arrange
        var stateMachineInstance = new StateMachineInstance();
        var stateMachineDefinition = GetStateMachineDefinition();

        // Act
        var configuredStateMachineInstance = stateMachineInstance.Configure(stateMachineDefinition, "StartState");

        // Assertion
        Assert.Equal("StartState", configuredStateMachineInstance.CurrentStateName);
    }

    private StateMachineDefinition GetStateMachineDefinition()
    {
        return new StateMachineDefinition
        {
            Id = "TestStateMachineDefinitionId",
            Name = "My test state machine definition",
            EntityType = "TestEntityType",
            IsActive = true,
            Version = "Version1",
            States = TestHepler.LoadArrayFromJsonFile("testStateMachineDefinition.json").ToObject<List<StateMachineState>>(),
            CreatedDate = new DateTime(2025, 02, 13),
            ModifiedDate = new DateTime(2025, 02, 13),
            CreatedBy = "Test Created By",
            ModifiedBy = "Test Modified By"
        };
    }

    private StateMachineDefinition GetStateMachineDefinitionBroken()
    {
        return new StateMachineDefinition
        {
            Id = "TestStateMachineDefinitionId",
            Name = "My test state machine definition",
            EntityType = "TestEntityType",
            IsActive = true,
            Version = "Version1",
            States = TestHepler.LoadArrayFromJsonFile("testStateMachineDefinitionBroken.json").ToObject<List<StateMachineState>>(),
            CreatedDate = new DateTime(2025, 02, 13),
            ModifiedDate = new DateTime(2025, 02, 13),
            CreatedBy = "Test Created By",
            ModifiedBy = "Test Modified By"
        };
    }
}
