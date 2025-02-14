using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Data.Models;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;
[ExcludeFromCodeCoverage]
public class StateMachineInstanceEntityTests
{
    [Fact]
    public void ConvertStateMachineInstanceEntityToModel_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var stateMachineInstanceEntity = new StateMachineInstanceEntity();

        // Act
        Action actual = () => stateMachineInstanceEntity.ToModel(null);

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Fact]
    public void ConvertStateMachineInstanceEntityFromModel_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var stateMachineInstanceEntity = new StateMachineInstanceEntity();

        // Act
        Action actual = () => stateMachineInstanceEntity.FromModel(null, null);

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Theory]
    [MemberData(nameof(Input))]
    public void ConvertStateMachineInstanceEntityToModelFromModel_NotNull_ReturnsActualValue(StateMachineInstanceEntity originalStateMachineInstanceEntity)
    {
        // Arrange
        var pkMap = new Mock<PrimaryKeyResolvingMap>();

        // Act
        var convertedStateMachineInstance = originalStateMachineInstanceEntity.ToModel(new StateMachineInstance());
        var convertedStateMachineInstanceEntity = new StateMachineInstanceEntity().FromModel(convertedStateMachineInstance, pkMap.Object);

        // Assertion
        Assert.Equal(originalStateMachineInstanceEntity.Id, convertedStateMachineInstanceEntity.Id);
        Assert.Equal(originalStateMachineInstanceEntity.EntityId, convertedStateMachineInstanceEntity.EntityId);
        Assert.Equal(originalStateMachineInstanceEntity.EntityType, convertedStateMachineInstanceEntity.EntityType);
        Assert.Equal(originalStateMachineInstanceEntity.StateMachineId, convertedStateMachineInstanceEntity.StateMachineId);
        Assert.Equal(originalStateMachineInstanceEntity.State, convertedStateMachineInstanceEntity.State);
        Assert.Equal(originalStateMachineInstanceEntity.CreatedDate, convertedStateMachineInstanceEntity.CreatedDate);
        Assert.Equal(originalStateMachineInstanceEntity.ModifiedDate, convertedStateMachineInstanceEntity.ModifiedDate);
        Assert.Equal(originalStateMachineInstanceEntity.CreatedBy, convertedStateMachineInstanceEntity.CreatedBy);
        Assert.Equal(originalStateMachineInstanceEntity.ModifiedBy, convertedStateMachineInstanceEntity.ModifiedBy);
    }

    [Fact]
    public void PatchStateMachineInstanceEntity_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var stateMachineInstanceEntity = new StateMachineDefinitionEntity();

        // Act
        Action actual = () => stateMachineInstanceEntity.Patch(null);

        // Assertion
        Assert.Throws<ArgumentNullException>(actual);
    }

    [Theory]
    [MemberData(nameof(Input))]
    public void PatchStateMachineInstanceEntity_NotNull_ReturnsActualValue(StateMachineInstanceEntity actualStateMachineInstanceEntity)
    {
        // Arrange
        var patchedStateMachineInstanceEntity = new StateMachineInstanceEntity();

        // Act
        actualStateMachineInstanceEntity.Patch(patchedStateMachineInstanceEntity);

        // Assertion
        Assert.Equal(actualStateMachineInstanceEntity.State, patchedStateMachineInstanceEntity.State);
    }

    public static TheoryData<StateMachineInstanceEntity> Input()
    {
        return new TheoryData<StateMachineInstanceEntity>()
        {
            new StateMachineInstanceEntity
            {
                Id = "TestStateMachineInstanceId",
                EntityId = "TestEntityId",
                EntityType = "TestEntityType",
                StateMachineId = "TestStateMachineDefinitionId",
                State = "Null",
                CreatedDate = new DateTime(2025, 02, 13),
                ModifiedDate = new DateTime(2025, 02, 13),
                CreatedBy = "Test Created By",
                ModifiedBy = "Test Modified By",
                StateMachineDefinition = new StateMachineDefinitionEntity
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
            }
        };
    }
}
