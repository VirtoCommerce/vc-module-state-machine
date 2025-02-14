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
public class FireStateMachineTriggerCommandTests
{
    [Fact]
    public void Handle_InalidRequest_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = new FireStateMachineTriggerCommand
        {
            StateMachineInstanceId = string.Empty,
            Trigger = "Finalize",
            EntityId = "TestEntityId"
        };
        var cancellationToken = new CancellationToken();

        // Act
        Action actual = () => GetCommandHandler().Handle(command, cancellationToken).GetAwaiter().GetResult();

        // Assertion
        Assert.Throws<OperationCanceledException>(actual);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsData()
    {
        // Arrange
        var command = new FireStateMachineTriggerCommand
        {
            StateMachineInstanceId = "TestStateMachineInstanceId",
            Trigger = "Finalize",
            EntityId = "TestEntityId"
        };
        var cancellationToken = new CancellationToken();

        // Act
        var stateMachineInstance = await GetCommandHandler().Handle(command, cancellationToken);

        // Assert
        stateMachineInstance.Id.Should().Be(command.StateMachineInstanceId);
    }

    private FireStateMachineTriggerCommandHandler GetCommandHandler()
    {
        IStateMachineInstanceService stateMachineInstanceService = new StateMachineInstanceServiceStub();
        return new FireStateMachineTriggerCommandHandler(stateMachineInstanceService);
    }
}
