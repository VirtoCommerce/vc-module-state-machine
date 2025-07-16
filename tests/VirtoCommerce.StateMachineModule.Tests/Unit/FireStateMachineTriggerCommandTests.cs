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
    public void Handle_EmptyStateMachineInstanceId_ThrowsArgumentNullException()
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
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Fact]
    public void Handle_EmptyTrigger_ThrowsArgumentNullException()
    {
        // Arrange
        var command = new FireStateMachineTriggerCommand
        {
            StateMachineInstanceId = "TestStateMachineInstanceId",
            Trigger = string.Empty,
            EntityId = "TestEntityId"
        };
        var cancellationToken = new CancellationToken();

        // Act
        Action actual = () => GetCommandHandler().Handle(command, cancellationToken).GetAwaiter().GetResult();

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Fact]
    public void Handle_EmptyEntityId_ThrowsArgumentNullException()
    {
        // Arrange
        var command = new FireStateMachineTriggerCommand
        {
            StateMachineInstanceId = "TestStateMachineInstanceId",
            Trigger = "Finalize",
            EntityId = string.Empty
        };
        var cancellationToken = new CancellationToken();

        // Act
        Action actual = () => GetCommandHandler().Handle(command, cancellationToken).GetAwaiter().GetResult();

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Fact]
    public void Handle_InalidStateMachineInstanceId_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = new FireStateMachineTriggerCommand
        {
            StateMachineInstanceId = "InvalidInstanceId",
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
        IStateMachineLocalizationSearchService stateMachineLocalizationSearchService = new StateMachineLocalizationSearchServiceStub();
        IStateMachineAttributeSearchService stateMachineAttributeSearchService = new StateMachineAttributeSearchServiceStub();
        return new FireStateMachineTriggerCommandHandler(stateMachineInstanceService, stateMachineLocalizationSearchService, stateMachineAttributeSearchService);
    }
}
