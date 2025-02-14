using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Data.Validators;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;
[ExcludeFromCodeCoverage]
public class StateMachineValidatorTest
{
    [Fact]
    public void Validate_EmptyName_ThrowsValidationException()
    {
        // Arrange
        var stateMachineValidator = new StateMachineValidator();
        var stateMachineDefinition = new StateMachineDefinition
        {
            Name = string.Empty,
            EntityType = "TestEntityType",
            States = new List<StateMachineState>()
        };

        // Act
        Action actual = () => stateMachineValidator.ValidateAndThrowAsync(stateMachineDefinition).GetAwaiter().GetResult();

        // Assertion
        Assert.Throws<ValidationException>(actual);
    }

    [Fact]
    public void Validate_EmptyEntityType_ThrowsValidationException()
    {
        // Arrange
        var stateMachineValidator = new StateMachineValidator();
        var stateMachineDefinition = new StateMachineDefinition
        {
            Name = "TestStateMachineDefinitionName",
            EntityType = string.Empty,
            States = new List<StateMachineState>()
        };

        // Act
        Action actual = () => stateMachineValidator.ValidateAndThrowAsync(stateMachineDefinition).GetAwaiter().GetResult();

        // Assertion
        Assert.Throws<ValidationException>(actual);
    }

    [Fact]
    public void Validate_EmptyStates_ThrowsValidationException()
    {
        // Arrange
        var stateMachineValidator = new StateMachineValidator();
        var stateMachineDefinition = new StateMachineDefinition
        {
            Name = "TestStateMachineDefinitionName",
            EntityType = "TestEntityType",
            States = null
        };

        // Act
        Action actual = () => stateMachineValidator.ValidateAndThrowAsync(stateMachineDefinition).GetAwaiter().GetResult();

        // Assertion
        Assert.Throws<ValidationException>(actual);
    }

}
