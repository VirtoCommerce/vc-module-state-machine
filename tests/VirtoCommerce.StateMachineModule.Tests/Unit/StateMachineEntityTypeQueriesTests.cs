using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Models.Conditions;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data.Queries;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;

[Trait("Category", "Unit")]
public class StateMachineEntityTypeQueriesTests
{
    private const string EntityTypeId = "VirtoCommerce.QuoteModule.Core.Models.QuoteRequest";

    [Fact]
    public async Task GetEntityTypes_ReturnsRegisteredTypes()
    {
        // Arrange
        IList<StateMachineEntityType> registered = [new StateMachineEntityType { Id = EntityTypeId, Name = "Quote request" }];
        var registrar = new Mock<IStateMachineEntityTypeRegistrar>();
        registrar.Setup(x => x.GetAll()).Returns(registered);
        var handler = new GetStateMachineEntityTypesQueryHandler(registrar.Object);

        // Act
        var result = await handler.Handle(new GetStateMachineEntityTypesQuery(), CancellationToken.None);

        // Assert
        Assert.Same(registered, result);
    }

    [Fact]
    public async Task GetEntityTypes_NullRequest_Throws()
    {
        // Arrange
        var handler = new GetStateMachineEntityTypesQueryHandler(Mock.Of<IStateMachineEntityTypeRegistrar>());

        // Act
        var act = () => handler.Handle(null, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task GetConditionTreePrototype_ReturnsRegistrarPrototypeForTheType()
    {
        // Arrange
        var prototype = new StateMachineConditionHasPermission();
        var registrar = new Mock<IStateMachineEntityTypeRegistrar>();
        registrar.Setup(x => x.GetConditionTreePrototype(EntityTypeId)).Returns(prototype);
        var handler = new GetStateMachineConditionTreePrototypeQueryHandler(registrar.Object);

        // Act
        var result = await handler.Handle(new GetStateMachineConditionTreePrototypeQuery { EntityTypeId = EntityTypeId }, CancellationToken.None);

        // Assert
        Assert.Same(prototype, result);
    }

    [Fact]
    public async Task GetConditionTreePrototype_TypeWithoutConditions_ReturnsNull()
    {
        // Arrange
        var handler = new GetStateMachineConditionTreePrototypeQueryHandler(Mock.Of<IStateMachineEntityTypeRegistrar>());

        // Act
        var result = await handler.Handle(new GetStateMachineConditionTreePrototypeQuery { EntityTypeId = EntityTypeId }, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetConditionTreePrototype_NullRequest_Throws()
    {
        // Arrange
        var handler = new GetStateMachineConditionTreePrototypeQueryHandler(Mock.Of<IStateMachineEntityTypeRegistrar>());

        // Act
        var act = () => handler.Handle(null, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }
}
