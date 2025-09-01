using System.Text.Json.Nodes;

namespace ConsoleMicrosoftJson.Extensions;

public static class JsonObjectExtensions
{
    /// <summary>
    /// Creates a deep clone of properties and puts them into an array based upon the text
    /// that the property starts with.  If indicated, the source property can be removed/deleted.
    /// </summary>
    /// <param name="source">The source object to scan</param>
    /// <param name="removeSourceProperty">Indicates if you want the source property removed/deleted</param>
    /// <param name="startsWith">the text that the property starts</param>
    /// <param name="caseSensitive">Indicates if the match should be case-sensitive or not</param>
    /// <returns>JsonArray of extracted objects (it could be empty if noting is found)</returns>
    public static JsonArray ExtractToArrayUsingStartsWith(this JsonObject source, bool removeSourceProperty, string startsWith,
        bool caseSensitive)
    {
        if (string.IsNullOrEmpty(startsWith))
            return new JsonArray();

        var comparison = caseSensitive ?
            StringComparison.Ordinal :
            StringComparison.OrdinalIgnoreCase;

        return source.ExtractToArray(removeSourceProperty,
            item => item.Key.StartsWith(startsWith, comparison));
    }

    /// <summary>
    /// Creates a deep clone of properties and puts them into an array based upon the text
    /// that the property starts with.  If indicated, the source property can be removed/deleted.
    /// </summary>
    /// <param name="source">The source object to scan</param>
    /// <param name="nodeCompareFunction">Allows you to write your own function
    /// to see if the current node should be extracted</param>
    /// <param name="removeSourceProperty">Indicates if you want the source property removed/deleted</param>
    /// <returns>JsonArray of extracted objects (it could be empty if noting is found)</returns>
    /// <example>
    /// This 
    /// {
    ///   "personOne": {
    ///     "name": "Alice",
    ///     "age": 30,
    ///     "city": "New York",
    ///     "spouse": {
    ///       "name": "John",
    ///       "age": 32,
    ///       "favoriteColors": [ "blue", "green" ],
    ///       "address": {
    ///         "street": "123 Main St",
    ///         "city": "New York",
    ///         "zip": "10001"
    ///       }
    ///     }
    ///   },
    ///   "personTwo": {
    ///     "name": "Bob",
    ///     "age": 25,
    ///     "city": "Los Angeles"
    ///   }
    /// }
    /// becomes this when using a function that returns true if the property name starts with "person"
    /// [
    ///   {
    ///     "name": "Alice",
    ///     "age": 30,
    ///     "city": "New York",
    ///     "spouse": {
    ///       "name": "John",
    ///       "age": 32,
    ///       "favoriteColors": [
    ///         "blue",
    ///         "green"
    ///       ],
    ///       "address": {
    ///         "street": "123 Main St",
    ///         "city": "New York",
    ///         "zip": "10001"
    ///       }
    ///     }
    ///   },
    ///   {
    ///     "name": "Bob",
    ///     "age": 25,
    ///     "city": "Los Angeles"
    ///   }
    /// ]
    /// </example>
    public static JsonArray ExtractToArray(this JsonObject source, bool removeSourceProperty,
        Func<KeyValuePair<string, JsonNode?>, bool> nodeCompareFunction)
    {
        var results = new JsonArray();
        var propertiesToRemove = new List<string>();

        foreach (var item in source)
        {
            if (item.Value != null && nodeCompareFunction(item))
            {
                results.Add(item.Value.DeepClone());

                if (removeSourceProperty)
                {
                    propertiesToRemove.Add(item.Key);
                }
            }
        }

        // Remove the properties after the loop to avoid modifying the collection while iterating
        foreach (var propertyName in propertiesToRemove)
        {
            source.Remove(propertyName);
        }

        return results;
    }

    /// <summary>
    /// Moves properties from the source object to the targetParent object if the property name
    /// matches the split criteria. The first part of the split property name will be used
    /// as the name of the new object and the second part will be used as the name of the
    /// property the new object that will receive the property found on the source
    /// (see example on the <seealso cref="UnflattenJsonObjectUsingFunc"/> method below
    /// for better understanding of what it will do).
    /// </summary>
    /// <param name="source">Source object</param>
    /// <param name="removeSourceProperty">Indicates if we should remove the property from the source object.</param>
    /// <param name="targetParent">The targetParent object where new properties will be created</param>
    /// <param name="splitDelimiter">The split delimiter (double underscore by default since that how
    /// AKS represents different levels with environment variables).</param>
    public static void UnflattenUsingSplitDelimiter(this JsonObject source,
        bool removeSourceProperty, JsonObject targetParent, string splitDelimiter = "__")
    {
        source.UnflattenJsonObjectUsingFunc(removeSourceProperty, targetParent,
            (key, value) =>
            {
                var parts = key.Split(splitDelimiter);
                return parts.Length == 2;
            },
            (key, value) =>
            {
                var parts = key.Split(splitDelimiter);
                return (parts[0], parts[1]);
            });
    }

    /// <summary>
    /// Examines all properties on the source for the delimiter pattern.  If any are found, they
    /// are moved to properties and the original are deleted.  The name of the new property is
    /// determined by the property split.  In other words, if the property name was config__version,
    /// a new object called config would be created on the current item and a version property would
    /// be created on config containing the new value.  If the config property already exists,
    /// version will be added to it (exceptions can occur if version already exists).
    /// After the source is processed, we walk through every property on the source and recursively
    /// call this method to process it and any children it may have.
    /// </summary>
    /// <param name="source">Source object</param>
    /// <param name="splitDelimiter">The split delimiter (double underscore by default since that how
    /// AKS represents different levels with environment variables).</param>
    public static void UnflattenRecursiveUsingSplitDelimiter(this JsonObject source, string splitDelimiter = "__")
    {
        // First unflatten the current level
        // We are forcing the removal of properties because we are about to iterate over all the children
        // again and we don't want to process the same property twice.
        UnflattenUsingSplitDelimiter(source, true, source, splitDelimiter);

        // Now recurse into any child objects
        foreach (var item in source)
        {
            if (item.Value is JsonObject childObject)
            {
                UnflattenRecursiveUsingSplitDelimiter(childObject, splitDelimiter);
            }
        }
    }

    /// <summary>
    /// Moves properties from the source object to the targetParent object.  On the
    /// targetParent object you will be asked for a new property name and it will be
    /// created or overwritten with the data being moved.
    /// </summary>
    /// <param name="source">The source of the properties to move.</param>
    /// <param name="removeSourceProperty">Indicates if the property should be removed from the source.</param>
    /// <param name="targetParent">The targetParent object where will create new property or overwrite
    /// an existing property based upon the name you return from newPropertyNameFunction.</param>
    /// <param name="nodeCompareFunction">The function used to determine if a property
    /// should be moved from the source</param>
    /// <param name="newPropertyNameFunction">The function that provides the new </param>
    /// <returns>True if anything was added or modified; otherwise, false</returns>
    /// <example>
    /// This
    /// {
    ///    "title": "Eternal Sunshine of a spotless mind",
    ///    "config__version": 3,
    ///    "config__address": {
    ///        "street": "123 Main St",
    ///        "city": "New York",
    ///        "zip": "10001"
    ///}
    /// becomes this if you split on the '__' delimiter, remove the source properties and
    /// return config as the target property name and address as the new property name.
    /// {
    ///     "title": "Eternal Sunshine of a spotless mind",
    ///     "config": {
    ///         "version": 3,
    ///         "address": {
    ///             "street": "123 Main St",
    ///             "city": "New York",
    ///             "zip": "10001"
    ///         }
    ///     }
    /// }
    /// </example>
    public static bool UnflattenJsonObjectUsingFunc(this JsonObject source, bool removeSourceProperty,
       JsonObject targetParent,
       Func<string, JsonNode?, bool> nodeCompareFunction,
       Func<string, JsonNode?, (string parentPropertyName, string newPropertyName)> newPropertyNameFunction)
    {
        var result = false;
        var properties = new Dictionary<string, bool>();

        // In case the source and targetParent are the same object, obtain the property names first
        // so that we avoid modifying the collection while iterating
        foreach (var item in source)
        {
            if (item.Value != null && nodeCompareFunction(item.Key, item.Value))
            {
                properties.Add(item.Key, removeSourceProperty);
            }
        }

        foreach (var item in properties)
        {
            var sourceProperty = source[item.Key];
            if (sourceProperty == null)
                continue;
            (string targetPropertyName, string newPropertyName) = newPropertyNameFunction(item.Key, sourceProperty);

            var targetPropertyObject = targetParent[targetPropertyName] as JsonObject;

            if (targetPropertyObject == null)
            {
                targetPropertyObject = new JsonObject();
                targetParent[targetPropertyName] = targetPropertyObject;
            }

            targetPropertyObject[newPropertyName] = sourceProperty.DeepClone();
            result = true;
        }

        // Remove the properties after the loop to avoid modifying the collection while iterating
        if (removeSourceProperty)
        {
            foreach (var property in properties)
            {
                source.Remove(property.Key);
            }
        }

        return result;
    }


    //public static void FlattenJsonObjectUsingFunc(this JsonObject source, bool removeSourceProperty,
    //    JsonObject targetParent,
    //    Func<string, JsonNode?, bool> nodeCompareFunction,
    //    Func<string, JsonNode?, (string parentPropertyName, string newPropertyName)> newPropertyNameFunction)
    //{
    //    var properties = new Dictionary<string, bool>();

    //    // In case the source and targetParent are the same object, obtain the property names first
    //    // so that we avoid modifying the collection while iterating
    //    foreach (var item in source)
    //    {
    //        if (item.Value != null && nodeCompareFunction(item.Key, item.Value))
    //        {
    //            properties.Add(item.Key, removeSourceProperty);
    //        }
    //    }

    //    foreach (var item in properties)
    //    {
    //        var sourceProperty = source[item.Key];
    //        if (sourceProperty == null)
    //            continue;
    //        (string targetPropertyName, string newPropertyName) = newPropertyNameFunction(item.Key, sourceProperty);

    //        var targetPropertyObject = targetParent[targetPropertyName] as JsonObject;

    //        if (targetPropertyObject == null)
    //        {
    //            targetPropertyObject = new JsonObject();
    //            targetParent[targetPropertyName] = targetPropertyObject;
    //        }

    //        targetPropertyObject[newPropertyName] = sourceProperty.DeepClone();
    //    }

    //    // Remove the properties after the loop to avoid modifying the collection while iterating
    //    foreach (var property in properties)
    //    {
    //        if (property.Value)
    //            source.Remove(property.Key);
    //    }
    //}
}