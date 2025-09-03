using System.Text.Json.Nodes;
#pragma warning disable CS0184

namespace ConsoleMicrosoftJson.Extensions;

public static class JsonNodeExtensions
{
    /// <summary>
    /// Finds a node within a JsonNode using a period-delimited path.
    /// Warning, we cannot navigate through arrays. If you need to target an array,
    /// the path must end at the array and T must be JsonArray.
    /// </summary>
    /// <typeparam name="T">The type that you expect at the period-delimited path</typeparam>
    /// <param name="source">The source object to scan</param>
    /// <param name="periodDelimitedPath">The period delimited path (yes, it's case-sensitive)</param>
    /// <returns>Null if it is not found; otherwise, the cast object type (e.g., JsonObject, JsonArray)</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static T? SelectNode<T>(this JsonNode source, string periodDelimitedPath) where T : JsonNode
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (string.IsNullOrEmpty(periodDelimitedPath))
            throw new ArgumentNullException(nameof(periodDelimitedPath));

        var parts = periodDelimitedPath.Split('.');
        JsonNode? currentNode = source;

        foreach (var part in parts)
        {
            if (currentNode is JsonObject currentObject)
            {
                if (!currentObject.ContainsKey(part))
                    return null;

                currentNode = currentObject[part];
            }
            else if (currentNode is JsonArray)
            {
                // If the user was targeting an array, return the array if T is JsonArray
                if (part == parts.Last() && typeof(T) is JsonArray)
                    return currentNode as T;

                throw new InvalidOperationException("Cannot navigate through arrays. " +
                     $"Invalid part '{part}' in path '{periodDelimitedPath}'.");
            }
        }

        return currentNode as T;
    }
}