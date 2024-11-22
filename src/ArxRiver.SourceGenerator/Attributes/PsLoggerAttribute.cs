namespace ArxRiver.SourceGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
                AllowMultiple = false, Inherited = false)]
public sealed class PsLoggerAttribute : Attribute
{
    public enum LoggerType
    {
        Json,
        File,
        Console,
        ColoredConsole
    }
    
    public LoggerType Type { get; set; } = LoggerType.Json;

    public PsLoggerAttribute(LoggerType loggerType)
    {
        Type = loggerType;
    }
}