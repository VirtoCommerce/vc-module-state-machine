using VirtoCommerce.StateMachineModule.Data.Queries;
using Xunit;

namespace VirtoCommerce.StateMachineModule.Tests.Unit;

[Trait("Category", "Unit")]
public class SearchStateMachineTextsQueryTests
{
    [Fact]
    public void LocalizationsToCriteria_CopiesAllFilters()
    {
        // Arrange
        var query = new SearchStateMachineLocalizationsQuery
        {
            DefinitionId = "Definition1",
            DefinitionIds = ["Definition1", "Definition2"],
            Item = "New",
            Locale = "en-US",
            ObjectIds = ["Localization1"],
            SearchPhrase = "phrase",
            Sort = "item:ASC",
            Skip = 20,
            Take = 10,
        };

        // Act
        var criteria = query.ToCriteria();

        // Assert
        Assert.Equal("Definition1", criteria.DefinitionId);
        Assert.Equal(["Definition1", "Definition2"], criteria.DefinitionIds);
        Assert.Equal("New", criteria.Item);
        Assert.Equal("en-US", criteria.Locale);
        Assert.Equal(["Localization1"], criteria.ObjectIds);
        Assert.Equal("phrase", criteria.SearchPhrase);
        Assert.Equal("item:ASC", criteria.Sort);
        Assert.Equal(20, criteria.Skip);
        Assert.Equal(10, criteria.Take);
    }

    [Fact]
    public void AttributesToCriteria_CopiesAllFilters()
    {
        // Arrange
        var query = new SearchStateMachineAttributesQuery
        {
            DefinitionId = "Definition1",
            DefinitionIds = ["Definition1", "Definition2"],
            Item = "New",
            AttributeKey = "Icon",
            ObjectIds = ["Attribute1"],
            SearchPhrase = "phrase",
            Sort = "item:ASC",
            Skip = 20,
            Take = 10,
        };

        // Act
        var criteria = query.ToCriteria();

        // Assert
        Assert.Equal("Definition1", criteria.DefinitionId);
        Assert.Equal(["Definition1", "Definition2"], criteria.DefinitionIds);
        Assert.Equal("New", criteria.Item);
        Assert.Equal("Icon", criteria.AttributeKey);
        Assert.Equal(["Attribute1"], criteria.ObjectIds);
        Assert.Equal("phrase", criteria.SearchPhrase);
        Assert.Equal("item:ASC", criteria.Sort);
        Assert.Equal(20, criteria.Skip);
        Assert.Equal(10, criteria.Take);
    }
}
