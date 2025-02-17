using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data.Commands;
using VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;
[ExcludeFromCodeCoverage]
public class CreateStateMachineInstanceCommandTests
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
        var command = new CreateStateMachineInstanceCommand();
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
        var command = new CreateStateMachineInstanceCommand
        {
            StateMachineDefinitionId = "TestStateMachineDefinitionId",
            StateMachineInstanceId = "TestStateMachineInstanceId",
            Entity = null
        };
        var cancellationToken = new CancellationToken();

        // Act
        var stateMachineInstance = await GetCommandHandler().Handle(command, cancellationToken);

        // Assert
        stateMachineInstance.Id.Should().Be(command.StateMachineInstanceId);
    }

    private CreateStateMachineInstanceCommandHandler GetCommandHandler()
    {
        IStateMachineInstanceService stateMachineInstanceService = new StateMachineInstanceServiceStub();
        return new CreateStateMachineInstanceCommandHandler(stateMachineInstanceService);
    }
}
