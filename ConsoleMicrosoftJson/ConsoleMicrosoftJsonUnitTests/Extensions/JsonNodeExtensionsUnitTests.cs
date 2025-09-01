using ConsoleMicrosoftJson.Extensions;
using ConsoleMicrosoftJsonUnitTests.Data;
using System.Text.Json.Nodes;

namespace ConsoleMicrosoftJsonUnitTests;

public class JsonNodeExtensionsUnitTest
{
    [Fact]
    public void SelectNode_CanFindJsonObject()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example1.json");

        // Act
        JsonObject? actual = sourceObject.SelectNode<JsonObject>("personOne.spouse.address");

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual["city"]?.ToString() == "New York");
    }

    [Fact]
    public void SelectNode_CanFindJsonArray()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example1.json");

        // Act
        JsonArray? actual = sourceObject.SelectNode<JsonArray>("personOne.spouse.favoriteColors");

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(2, actual.Count);
        Assert.True(actual[0]?.ToString() == "blue");
    }

    [Fact]
    public void SelectNode_ThrowsAnException_WhenAttemptingToNavigateAnArray()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example1.json");

        // Act
        var actual = Assert.Throws<InvalidOperationException>(() => sourceObject.SelectNode<JsonArray>("musicians.name"));

        // Assert
        Assert.Contains("Cannot navigate through arrays.", actual.Message);
    }
}