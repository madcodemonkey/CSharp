using ConsoleMicrosoftJson.Extensions;
using ConsoleMicrosoftJsonUnitTests.Data;
using System.Text.Json.Nodes;

namespace ConsoleMicrosoftJsonUnitTests;

public class JsonObjectExtensionsUnitTest
{
    #region ExtractToArrayUsingStartsWith
    [Fact]
    public void ExtractToArrayUsingStartsWith_CanFindItems_WhenCaseSensitiveIsTrue()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example1.json");

        // Act
        JsonArray actual = sourceObject.ExtractToArrayUsingStartsWith(true, "perSON", true);

        // Assert
        Assert.NotNull(actual);
        Assert.Single(actual);
        Assert.True(actual.Any(person => person["name"]?.ToString() == "James"));

        Assert.Null(sourceObject["perSONThree"]); // Should be removed
        Assert.NotNull(sourceObject["personOne"]); // Should NOT be removed
        Assert.NotNull(sourceObject["personTwo"]); // Should NOT be removed
    }

    [Fact]
    public void ExtractToArrayUsingStartsWith_CanFindItems_WhenCaseSensitiveIsFalse()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example1.json");

        // Act
        JsonArray actual = sourceObject.ExtractToArrayUsingStartsWith(true, "perSON", false);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(3, actual.Count);
        Assert.True(actual.Any(person => person["name"]?.ToString() == "James"));
        Assert.True(actual.Any(person => person["name"]?.ToString() == "Alice"));
        Assert.True(actual.Any(person => person["name"]?.ToString() == "Bob"));

        Assert.Null(sourceObject["perSONThree"]);
        Assert.Null(sourceObject["personOne"]);
        Assert.Null(sourceObject["personTwo"]);
    }

    [Fact]
    public void ExtractToArrayUsingStartsWith_DoesNotRemoveItems_WhenRemoveSourcePropertyIsFalse()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example1.json");

        // Act
        JsonArray actual = sourceObject.ExtractToArrayUsingStartsWith(false, "perSON", false);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(3, actual.Count);
        Assert.True(actual.Any(person => person["name"]?.ToString() == "James"));
        Assert.True(actual.Any(person => person["name"]?.ToString() == "Alice"));
        Assert.True(actual.Any(person => person["name"]?.ToString() == "Bob"));

        Assert.NotNull(sourceObject["perSONThree"]);
        Assert.NotNull(sourceObject["personOne"]);
        Assert.NotNull(sourceObject["personTwo"]);
    }

    [Fact]
    public void ExtractToArrayUsingStartsWith_DeepCloneCanReadCopyAllItems_WhenThereAreSeveralLevelsDeep()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example1.json");

        // Act
        JsonArray actual = sourceObject.ExtractToArrayUsingStartsWith(true, "personO", true);

        // Assert
        Assert.NotNull(actual);
        Assert.Single(actual);
        var person = actual[0] as JsonObject;
        Assert.NotNull(person);
        Assert.Equal("Alice", person["name"]?.ToString());
        var spouse = person["spouse"] as JsonObject;
        Assert.NotNull(spouse);
        Assert.Equal("John", spouse["name"]?.ToString());
        var favColors = spouse["favoriteColors"] as JsonArray;
        Assert.NotNull(favColors);
        Assert.Equal(2, favColors.Count);
        var address = spouse["address"] as JsonObject;
        Assert.NotNull(address);
        Assert.Equal("123 Main St", address["street"]?.ToString());
    }
    #endregion // ExtractToArrayUsingStartsWith

    #region UnflattenJsonObjectUsingSplit
    [Fact]
    public void UnflattenJsonObjectUsingSplit_CanCreateConfig()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example2a.json");

        // Act
        sourceObject.UnflattenJsonObjectUsingSplit(true, sourceObject, "__");

        // var asString = sourceObject.ToString();

        // Assert
        Assert.NotNull(sourceObject["title"]);
        Assert.Null(sourceObject["config_version"]);
        Assert.Null(sourceObject["config_address"]);
        var config = sourceObject["config"] as JsonObject;
        Assert.NotNull(config);
        Assert.NotNull(config["version"]);
        Assert.NotNull(config["address"]);
    }

    [Fact]
    public void UnflattenJsonObjectUsingSplit_CanCreateConfig_WhenTargetIsNotSource()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example2a.json");
        var parentObject = sourceObject["misc"] as JsonObject;
        Assert.NotNull(parentObject);

        // Act
        sourceObject.UnflattenJsonObjectUsingSplit(true, parentObject, "__");

        // var asString = sourceObject.ToString();

        // Assert
        Assert.NotNull(sourceObject["title"]);
        Assert.Null(sourceObject["config_version"]);
        Assert.Null(sourceObject["config_address"]);
        var config = parentObject["config"] as JsonObject;
        Assert.NotNull(config);
        Assert.NotNull(config["version"]);
        Assert.NotNull(config["address"]);
    }
    #endregion // UnflattenJsonObjectUsingSplit

}