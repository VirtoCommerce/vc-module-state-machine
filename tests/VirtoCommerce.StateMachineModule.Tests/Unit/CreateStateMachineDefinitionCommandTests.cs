using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data.Commands;
using VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;
[ExcludeFromCodeCoverage]
public class CreateStateMachineDefinitionCommandTests
{
    [Fact]
    public void Handle_NullCommandRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        // Act
        Action actual = () => GetCommandHandler().Handle(null, cancellationToken).GetAwaiter().GetResult();

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Fact]
    public void Handle_InvalidRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var command = new CreateStateMachineDefinitionCommand();
        var cancellationToken = new CancellationToken();

        // Act
        Action actual = () => GetCommandHandler().Handle(command, cancellationToken).GetAwaiter().GetResult();

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsData()
    {
        // Arrange
        var command = new CreateStateMachineDefinitionCommand
        {
            Definition = new StateMachineDefinition
            {
                Id = "TestStateMachineDefinitionId",
                Name = "My test state machine definition",
                EntityType = "TestEntityType",
                IsActive = true,
                Version = "Version1",
            }
        };
        var cancellationToken = new CancellationToken();

        // Act
        var stateMachineDefinition = await GetCommandHandler().Handle(command, cancellationToken);

        // Assert
        stateMachineDefinition.Id.Should().Be(command.Definition.Id);
        stateMachineDefinition.Name.Should().Be(command.Definition.Name);
        stateMachineDefinition.EntityType.Should().Be(command.Definition.EntityType);
        stateMachineDefinition.IsActive.Should().Be(command.Definition.IsActive);
        stateMachineDefinition.Version.Should().Be(command.Definition.Version);
    }

    private CreateStateMachineDefinitionCommandHandler GetCommandHandler()
    {
        IStateMachineDefinitionService stateMachineDefinitionService = new StateMachineDefinitionServiceStub();
        return new CreateStateMachineDefinitionCommandHandler(stateMachineDefinitionService);
    }
}
