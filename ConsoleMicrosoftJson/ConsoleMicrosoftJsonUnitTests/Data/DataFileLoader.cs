using System.Reflection;
using System.Text.Json.Nodes;

namespace ConsoleMicrosoftJsonUnitTests.Data;

internal class DataFileLoader
{
    public static Stream GetFileDataAsStream(string embeddedFileName)
    {
        // Based on: https://adamprescott.net/2012/07/26/files-as-embedded-resources-in-unit-tests/
        var asm = Assembly.GetExecutingAssembly();
        // Note 1: When assembly and namespace differ it appears that his is actually the namespace!
        // Note 2: Recently, Assembly Name + Name of the folder it was in! 
        var resource = $"ConsoleMicrosoftJsonUnitTests.Data.{embeddedFileName}";
        var stream = asm.GetManifestResourceStream(resource);

        if (stream == null)
        {
            throw new FileNotFoundException($"Could not find embedded resource '{resource}'. " +
                 $" On the file's properties, make sure the file's build action is 'Embedded Resource'");
        }

        return stream;
    }

    public static MemoryStream GetFileDataAsMemoryStream(string embeddedFileName)
    {
        using (var stream = GetFileDataAsStream(embeddedFileName))
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }

    public static byte[] GetFileDataAsByteArray(string embeddedFileName)
    {
        using (var ms = GetFileDataAsMemoryStream(embeddedFileName))
        {
            return ms.ToArray();
        }
    }

    public static string GetFileDataAsString(string embeddedFileName)
    {
        string result;

        using (var ms = GetFileDataAsMemoryStream(embeddedFileName))
        using (var sr = new StreamReader(ms))
        {
            result = sr.ReadToEnd();
        }

        return result;
    }

    public static JsonObject GetFileDataAsJsonObject(string embeddedFileName)
    {
        string data = GetFileDataAsString(embeddedFileName);
        var parsedObject = JsonNode.Parse(data);
        if (parsedObject is JsonObject jsonObject)
        {
            return jsonObject;
        }

        throw new InvalidDataException($"Unable to convert the specified embedded file ({embeddedFileName}) into a JsonObject.");
    }

    public static JsonArray GetFileDataAsJsonArray(string embeddedFileName)
    {
        string data = GetFileDataAsString(embeddedFileName);
        var parsedObject = JsonNode.Parse(data);
        if (parsedObject is JsonArray jsonArray)
        {
            return jsonArray;
        }

        throw new InvalidDataException($"Unable to convert the specified embedded file ({embeddedFileName}) into a JsonArray.");
    }
}