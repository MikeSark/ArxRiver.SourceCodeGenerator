using System.CodeDom.Compiler;
using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Attributes;
using ArxRiver.SourceGenerator.Configuration;
using ArxRiver.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace ArxRiver.SourceGenerator.Generators.NLogger;

internal static class PsNLogBuilder
{
    internal static void Build(IndentedTextWriter writer, TypeSymbolModel type, ImmutableArray<PropertySymbolModel> properties)
    {
        var parameterValue = GetAttributeType(type);
        switch (parameterValue)
        {
            case PsLoggerAttribute.LoggerType.Json:
                BuildJsonLogger.Build(writer, type, properties);
                break;

            case PsLoggerAttribute.LoggerType.File:
                BuildFileLogger.Build(writer, type, properties);
                break;

            case PsLoggerAttribute.LoggerType.Console:
                BuildConsoleLogger.Build(writer, type, properties);
                break;

            case PsLoggerAttribute.LoggerType.ColoredConsole:
                BuildColoredConsoleLogger.Build(writer, type, properties);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static PsLoggerAttribute.LoggerType GetAttributeType(TypeSymbolModel type)
    {
        // find the attribute that was added to the symbol and get the logger type value
        var loggerAttribute = type
            .TypeSymbol
            .GetAttributes()
            .FirstOrDefault(x => x.AttributeClass!.Name.Equals(GeneratorConstants.PsLoggerAttributeName, StringComparison.InvariantCultureIgnoreCase));

        var parameterValue = (PsLoggerAttribute.LoggerType)(loggerAttribute?.ConstructorArguments[0].Value ?? 0);
        return parameterValue;
    }
}