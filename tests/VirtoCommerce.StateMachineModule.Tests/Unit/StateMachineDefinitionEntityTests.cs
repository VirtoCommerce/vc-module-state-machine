using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Data.Models;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;
[ExcludeFromCodeCoverage]
public class StateMachineDefinitionEntityTests
{
    [Fact]
    public void ConvertStateMachineDefinitionEntityToModel_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var stateMachineDefinitionEntity = new StateMachineDefinitionEntity();

        // Act
        Action actual = () => stateMachineDefinitionEntity.ToModel(null);

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Fact]
    public void ConvertStateMachineDefinitionEntityFromModel_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var stateMachineDefinitionEntity = new StateMachineDefinitionEntity();

        // Act
        Action actual = () => stateMachineDefinitionEntity.FromModel(null, null);

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Theory]
    [MemberData(nameof(Input))]
    public void ConvertStateMachineDefinitionEntityToModelFromModel_NotNull_ReturnsActualValue(StateMachineDefinitionEntity originalStateMachineDefinitionEntity)
    {
        // Arrange
        var pkMap = new Mock<PrimaryKeyResolvingMap>();

        // Act
        var convertedStateMachineDefinition = originalStateMachineDefinitionEntity.ToModel(new StateMachineDefinition());
        var convertedStateMachineDefinitionEntity = new StateMachineDefinitionEntity().FromModel(convertedStateMachineDefinition, pkMap.Object);

        // Assertion
        Assert.Equal(originalStateMachineDefinitionEntity.Id, convertedStateMachineDefinitionEntity.Id);
        Assert.Equal(originalStateMachineDefinitionEntity.Name, convertedStateMachineDefinitionEntity.Name);
        Assert.Equal(originalStateMachineDefinitionEntity.EntityType, convertedStateMachineDefinitionEntity.EntityType);
        Assert.Equal(originalStateMachineDefinitionEntity.IsActive, convertedStateMachineDefinitionEntity.IsActive);
        Assert.Equal(originalStateMachineDefinitionEntity.Version, convertedStateMachineDefinitionEntity.Version);
        Assert.Equal(originalStateMachineDefinitionEntity.StatesSerialized, convertedStateMachineDefinitionEntity.StatesSerialized);
        Assert.Equal(originalStateMachineDefinitionEntity.CreatedDate, convertedStateMachineDefinitionEntity.CreatedDate);
        Assert.Equal(originalStateMachineDefinitionEntity.ModifiedDate, convertedStateMachineDefinitionEntity.ModifiedDate);
        Assert.Equal(originalStateMachineDefinitionEntity.CreatedBy, convertedStateMachineDefinitionEntity.CreatedBy);
        Assert.Equal(originalStateMachineDefinitionEntity.ModifiedBy, convertedStateMachineDefinitionEntity.ModifiedBy);
    }

    [Fact]
    public void PatchStateMachineDefinitionEntity_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var stateMachineDefinitionEntity = new StateMachineDefinitionEntity();

        // Act
        Action actual = () => stateMachineDefinitionEntity.Patch(null);

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Theory]
    [MemberData(nameof(Input))]
    public void PatchStateMachineDefinitionEntity_NotNull_ReturnsActualValue(StateMachineDefinitionEntity actualStateMachineDefinitionEntity)
    {
        // Arrange
        var patchedStateMachineDefinitionEntity = new StateMachineDefinitionEntity();

        // Act
        actualStateMachineDefinitionEntity.Patch(patchedStateMachineDefinitionEntity);

        // Assertion
        Assert.Equal(actualStateMachineDefinitionEntity.Name, patchedStateMachineDefinitionEntity.Name);
        Assert.Equal(actualStateMachineDefinitionEntity.EntityType, patchedStateMachineDefinitionEntity.EntityType);
        Assert.Equal(actualStateMachineDefinitionEntity.IsActive, patchedStateMachineDefinitionEntity.IsActive);
        Assert.Equal(actualStateMachineDefinitionEntity.Version, patchedStateMachineDefinitionEntity.Version);
        Assert.Equal(actualStateMachineDefinitionEntity.StatesSerialized, patchedStateMachineDefinitionEntity.StatesSerialized);
    }

    public static TheoryData<StateMachineDefinitionEntity> Input()
    {
        return new TheoryData<StateMachineDefinitionEntity>()
        {
            new StateMachineDefinitionEntity
            {
                Id = "TestStateMachineDefinitionId",
                Name = "My test state machine definition",
                EntityType = "TestEntityType",
                IsActive = true,
                Version = "Version1",
                StatesSerialized = TestHepler.LoadArrayFromJsonFile("testStateMachineDefinition.json").ToString(),
                CreatedDate = new DateTime(2025, 02, 13),
                ModifiedDate = new DateTime(2025, 02, 13),
                CreatedBy = "Test Created By",
                ModifiedBy = "Test Modified By"
            }
        };
    }
}

