using System;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data.Services;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;

[Trait("Category", "Unit")]
public class StateMachineEntityTypeRegistrarTests
{
    private const string OrderType = "VirtoCommerce.OrdersModule.Core.Model.CustomerOrder";

    private class QuoteRequest;

    private class QuoteRequestConditionTreePrototype : ConditionTree;

    [Fact]
    public void GetAll_NothingRegistered_ReturnsEmpty()
    {
        // Arrange
        var registrar = new StateMachineEntityTypeRegistrar();

        // Act
        var result = registrar.GetAll();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Register_ReturnsTypesInRegistrationOrder()
    {
        // Arrange
        var registrar = new StateMachineEntityTypeRegistrar();

        // Act
        registrar.Register(OrderType, "Customer order");
        registrar.Register("Other.Type", "Other");

        // Assert
        Assert.Equal([OrderType, "Other.Type"], registrar.GetAll().Select(x => x.Id));
        Assert.Equal(["Customer order", "Other"], registrar.GetAll().Select(x => x.Name));
    }

    [Fact]
    public void Register_Generic_UsesFullTypeNameWithoutConditions()
    {
        // Arrange
        var registrar = new StateMachineEntityTypeRegistrar();

        // Act
        registrar.Register<QuoteRequest>("Quote request");

        // Assert
        var result = Assert.Single(registrar.GetAll());
        Assert.Equal(typeof(QuoteRequest).FullName, result.Id);
        Assert.Equal("Quote request", result.Name);
        Assert.False(result.HasConditionTree);
    }

    [Fact]
    public void Register_WithPrototype_HasConditionTree()
    {
        // Arrange
        var registrar = new StateMachineEntityTypeRegistrar();

        // Act
        registrar.Register<QuoteRequest, QuoteRequestConditionTreePrototype>("Quote request");

        // Assert
        var result = Assert.Single(registrar.GetAll());
        Assert.True(result.HasConditionTree);
    }

    [Fact]
    public void Register_SameIdIgnoringCase_ReplacesType()
    {
        // Arrange
        var registrar = new StateMachineEntityTypeRegistrar();
        registrar.Register(OrderType, "Customer order");

        // Act
        registrar.Register(OrderType.ToUpperInvariant(), "Order");

        // Assert
        var result = Assert.Single(registrar.GetAll());
        Assert.Equal("Order", result.Name);
    }

    [Fact]
    public void Register_ThenChangeOriginal_KeepsRegisteredValues()
    {
        // Arrange
        var registrar = new StateMachineEntityTypeRegistrar();
        var entityType = new StateMachineEntityType { Id = OrderType, Name = "Customer order" };

        // Act
        registrar.Register(entityType);
        entityType.Name = "Changed";
        registrar.GetAll()[0].Name = "Changed too";

        // Assert
        Assert.Equal("Customer order", registrar.GetAll()[0].Name);
    }

    [Fact]
    public void Register_Null_Throws()
    {
        // Arrange
        var registrar = new StateMachineEntityTypeRegistrar();

        // Act
        var act = () => registrar.Register(null);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Theory]
    [InlineData(null, "Name")]
    [InlineData(" ", "Name")]
    [InlineData(OrderType, null)]
    [InlineData(OrderType, "")]
    public void Register_EmptyIdOrName_Throws(string id, string name)
    {
        // Arrange
        var registrar = new StateMachineEntityTypeRegistrar();

        // Act
        var act = () => registrar.Register(id, name);

        // Assert
        Assert.ThrowsAny<ArgumentException>(act);
        Assert.Empty(registrar.GetAll());
    }

    [Fact]
    public void GetConditionTreePrototype_RegisteredWithPrototype_ReturnsNewInstanceEachTime()
    {
        // Arrange
        var registrar = new StateMachineEntityTypeRegistrar();
        registrar.Register<QuoteRequest, QuoteRequestConditionTreePrototype>("Quote request");

        // Act
        var first = registrar.GetConditionTreePrototype(typeof(QuoteRequest).FullName!.ToLowerInvariant());
        var second = registrar.GetConditionTreePrototype(typeof(QuoteRequest).FullName);

        // Assert
        Assert.IsType<QuoteRequestConditionTreePrototype>(first);
        Assert.IsType<QuoteRequestConditionTreePrototype>(second);
        Assert.NotSame(first, second);
    }

    [Theory]
    [InlineData(OrderType)]
    [InlineData("Unknown.Type")]
    [InlineData(null)]
    public void GetConditionTreePrototype_NoPrototypeOrUnknownType_ReturnsNull(string entityTypeId)
    {
        // Arrange
        var registrar = new StateMachineEntityTypeRegistrar();
        registrar.Register(OrderType, "Customer order");

        // Act
        var result = registrar.GetConditionTreePrototype(entityTypeId);

        // Assert
        Assert.Null(result);
    }
}
