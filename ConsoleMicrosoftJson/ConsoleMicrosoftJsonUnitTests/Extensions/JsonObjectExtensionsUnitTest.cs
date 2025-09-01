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
        Assert.Contains(actual, person => (person as JsonObject)?["name"]?.ToString() == "James");

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
        Assert.Contains(actual, person => (person as JsonObject)?["name"]?.ToString() == "James");
        Assert.Contains(actual, person => (person as JsonObject)?["name"]?.ToString() == "Alice");
        Assert.Contains(actual, person => (person as JsonObject)?["name"]?.ToString() == "Bob");

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
        Assert.Contains(actual, person => (person as JsonObject)?["name"]?.ToString() == "James");
        Assert.Contains(actual, person => (person as JsonObject)?["name"]?.ToString() == "Alice");
        Assert.Contains(actual, person => (person as JsonObject)?["name"]?.ToString() == "Bob");

        Assert.NotNull(sourceObject["perSONThree"]);
        Assert.NotNull(sourceObject["personOne"]);
        Assert.NotNull(sourceObject["personTwo"]);
    }


    [Fact]
    public void ExtractToArrayUsingStartsWith_ExtractsArraysAsItems_WhenAnArrayIsTargeted()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example1.json");

        // Act
        JsonArray actual = sourceObject.ExtractToArrayUsingStartsWith(false, "musicians", false);

        // var asString = actual.ToString();

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(2, actual.Count);
        Assert.Contains(actual, person => (person as JsonObject)?["name"]?.ToString() == "John Doe");
        Assert.Contains(actual, person => (person as JsonObject)?["name"]?.ToString() == "Jane Smith");

        Assert.NotNull(sourceObject["musicians"]);
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

    #region UnflattenUsingSplitDelimiter
    [Fact]
    public void UnflattenUsingSplitDelimiter_CanCreateConfig()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example2a.json");

        // Act
        sourceObject.UnflattenUsingSplitDelimiter(true, sourceObject, splitDelimiter: "__");

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
    public void UnflattenUsingSplitDelimiter_CanCreateConfig_WhenTargetIsNotSource()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example2a.json");
        var parentObject = sourceObject["misc"] as JsonObject;
        Assert.NotNull(parentObject);

        // Act
        sourceObject.UnflattenUsingSplitDelimiter(true, parentObject, splitDelimiter: "__");

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
    #endregion // UnflattenUsingSplitDelimiter

    #region UnflattenRecursiveUsingSplitDelimiter
    [Fact]
    public void UnflattenRecursiveUsingSplitDelimiter_CanCreateSubObjects()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example2a.json");

        // Act
        sourceObject.UnflattenRecursiveUsingSplitDelimiter(splitDelimiter: "__");

        var asString = sourceObject.ToString();

        // Assert
        // Level 0: source level
        Assert.NotNull(sourceObject["title"]);
        Assert.Null(sourceObject["config_version"]);
        Assert.Null(sourceObject["config_address"]);
        // Level 1: source's config node
        var config = sourceObject["config"] as JsonObject;
        Assert.NotNull(config);
        Assert.NotNull(config["version"]);
        Assert.NotNull(config["address"]);
        // Level 2: config's address node
        var address = config["address"] as JsonObject;
        Assert.NotNull(address);
        // Level 3: address's notes node
        var notes = address["notes"] as JsonObject;
        Assert.NotNull(notes);
        Assert.NotNull(notes["version"]);
        Assert.NotNull(notes["mail_instructions"]);
    }

    [Fact]
    public void UnflattenRecursiveUsingSplitDelimiter_CanHandleArrays()
    {
        // Arrange
        var sourceObject = DataFileLoader.GetFileDataAsJsonObject("Example3.json");

        // Act
        sourceObject.UnflattenRecursiveUsingSplitDelimiter(splitDelimiter: "__");

        // var asString = sourceObject.ToString();

        // Assert
        // Level 0: source level
        Assert.NotNull(sourceObject);
        Assert.Equal("Bob Marley", sourceObject["name"]?.ToString());

        // Level 1: source's concerts node
        var birthObject = sourceObject["birth"] as JsonObject;
        Assert.NotNull(birthObject);
        Assert.Equal("1945-02-06", birthObject["date"]?.ToString());
        // Level 2: birth's place node
        var placeObject = birthObject["place"] as JsonObject;
        Assert.NotNull(placeObject);
        Assert.Equal("Nine Mile", placeObject["city"]?.ToString());

        // Level 1: source's concerts node
        var concertsArray = sourceObject["concerts"] as JsonArray;
        Assert.NotNull(concertsArray);
        Assert.Equal(3, concertsArray.Count);
        Assert.Contains(concertsArray, concert => (concert as JsonObject)?["venue"]?.ToString() == "Majestic Theater");
        Assert.Contains(concertsArray, concert => (concert as JsonObject)?["venue"]?.ToString() == "State Theater");
        Assert.Contains(concertsArray, concert => (concert as JsonObject)?["venue"]?.ToString() == "Lyceum Theatre");
    }
    #endregion
}