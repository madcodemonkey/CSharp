namespace ConsoleMicrosoftJson;

public class UnflattenObjectInformation
{
    private readonly string? _targetPropertyName;

    /// <summary>Default Constructor</summary>
    public UnflattenObjectInformation() { }

    /// <summary>Constructor helper</summary>
    public UnflattenObjectInformation(string propertyName, string targetObjectName, string? targetPropertyName, bool isArray)
    {
        PropertyName = propertyName;
        TargetObjectName = targetObjectName;
        TargetPropertyName = targetPropertyName;
        IsArray = isArray;
    }

    /// <summary>
    /// The property name on the source
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// The name of the object or array that will be created on the source
    /// </summary>
    public string TargetObjectName { get; set; }

    /// <summary>
    /// The name of the object or array that will be created on the source
    /// </summary>
    public string? TargetPropertyName { get; set; }

    /// <summary>
    /// Indicates if the target object is an array
    /// </summary>
    public bool IsArray { get; set; }
}