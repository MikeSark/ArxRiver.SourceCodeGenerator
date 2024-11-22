using System.CodeDom.Compiler;
using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;

namespace ArxRiver.SourceGenerator.Generators.NLogger;

internal static class BuildJsonExtension
{

    private static readonly string[] _systemUsings =
    [
        "System",
        "NLog",
        "System.Text.Json",
    ];
    
    internal static void Build(IndentedTextWriter writer, TypeSymbolModel type)
    {
        WriteUsings(writer);

        if (type.ContainingNamespace is not null)
        {
            writer.WriteLines($"namespace {type.ContainingNamespace};");
        }

        writer.WriteLine("public static class NLoggerExtensions");
        writer.WriteLine("{");
        writer.Indent++;

        writer.WriteLine("public static void LogJsonInfo<T>(this NLog.Logger logger, T model, params object[] args) =>");
        writer.WriteLine("logger.LogJson(LogLevel.Warn, model, args);");
        writer.WriteLine();

        writer.WriteLine("public static void LogJsonWarning<T>(this NLog.Logger logger, T model, params object[] args) =>");
        writer.WriteLine("logger.LogJson(LogLevel.Warn, model, args);");
        writer.WriteLine();

        writer.WriteLine("public static void LogJsonError<T>(this NLog.Logger logger, T model, Exception? exception = null, params object[] args) =>");
        writer.WriteLine("logger.LogJson(LogLevel.Error, model, exception, args);");
        writer.WriteLine();

        writer.WriteLine("public static void LogJsonDebug<T>(this NLog.Logger logger, T model, Exception? exception = null, params object[] args) =>");
        writer.WriteLine("logger.LogJson(LogLevel.Debug, model, args);");
        writer.WriteLine();

        writer.WriteLine("public static void LogJsonTrace<T>(this NLog.Logger logger, T model, params object[] args) =>");
        writer.WriteLine("logger.LogJson(LogLevel.Trace, model, args);");
        writer.WriteLine();


        writer.WriteLines($$"""
                            /// <summary>
                            /// Logs a message at the specified log level with the provided model serialized as JSON.
                            /// </summary>
                            /// <typeparam name="T">The type of the model to be logged.</typeparam>
                            /// <param name="level">The log level at which to log the message.</param>
                            /// <param name="model">The model to be serialized and logged.</param>
                            /// <param name="args">Optional arguments to format the log message.</param>
                            public static void LogJson<T>(this NLog.Logger logger, LogLevel level, T model, params object[] args)
                            {
                                if (!logger.IsEnabled(level))
                                    return;
                            
                                var jsonMessage = JsonSerializer.Serialize(model);
                                logger.Log(level, jsonMessage, args);
                            }

                            /// <summary>
                            /// Logs a message at the specified log level with the provided model serialized as JSON.
                            /// </summary>
                            /// <typeparam name="T">The type of the model to be serialized and logged.</typeparam>
                            /// <param name="level">The log level at which to log the message.</param>
                            /// <param name="model">The model object to be serialized to JSON and logged.</param>
                            public static void LogJson<T>(this NLog.Logger logger, LogLevel level, T model)
                            {
                                if (!logger.IsEnabled(level))
                                    return;
                            
                                var jsonMessage = JsonSerializer.Serialize(model);
                                logger.Log(level, jsonMessage);
                            }
                            """);

        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void WriteUsings(IndentedTextWriter writer)
    {
        var usings = _systemUsings.ToList();
        writer.WriteLine(string.Join("\r\n", usings.Distinct().Select(u => $"using {u};")));
    }

}