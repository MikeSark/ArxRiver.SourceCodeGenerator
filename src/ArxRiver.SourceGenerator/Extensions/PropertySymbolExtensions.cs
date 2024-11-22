using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArxRiver.SourceGenerator.Extensions;

internal static class PropertySymbolExtensions
{
    internal static bool IsPropertyClonable(this IPropertySymbol propertySymbol)
    {
        return propertySymbol is { SetMethod: not null, IsReadOnly: false };
    }

    internal static bool IsGenericWithClassArgument(this IPropertySymbol propertySymbol)
    {
        var symbolType = (INamedTypeSymbol)propertySymbol.Type;
        return symbolType.IsGenericType &&
               symbolType.TypeArguments.Length == 1 &&
               symbolType.TypeArguments[0].TypeKind == TypeKind.Class;
    }
}