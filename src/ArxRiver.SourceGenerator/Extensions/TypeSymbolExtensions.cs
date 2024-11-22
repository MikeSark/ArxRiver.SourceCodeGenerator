using ArxRiver.SourceGenerator.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArxRiver.SourceGenerator.Extensions;

internal static class TypeSymbolExtensions
{
    internal static string GetFullyQualifiedName(this ITypeSymbol symbol)
    {
        var symbolFormatter = SymbolDisplayFormat
            .FullyQualifiedFormat.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        return symbol.ToDisplayString(symbolFormatter);
    }

    internal static string BuilderClassName(this ITypeSymbol symbol) =>
        symbol.Name.ToSafeClassName();

    internal static bool IsCloneableType(this ITypeSymbol symbol)
    {
        return
            symbol.IsReferenceType &&
            symbol.IsRecord || (symbol.TypeKind == TypeKind.Class && symbol.TypeKind != TypeKind.Enum) &&
            !symbol.IsAbstract &&
            !symbol.IsSealed &&
            symbol.SpecialType == SpecialType.None &&
            symbol.CanBeReferencedByName;
    }

    internal static bool IsGenericWithClassArgument(this ITypeSymbol symbol)
    {
        var symbolType = (INamedTypeSymbol)symbol;
        return symbolType.IsGenericType &&
               symbolType.TypeArguments.Length == 1 &&
               symbolType.TypeArguments[0].TypeKind == TypeKind.Class;
    }


    internal static bool HasAttribute(this ITypeSymbol symbol, string attributeName) =>
        symbol.GetAttributes().Any(attributeData => attributeData.AttributeClass?.Name == attributeName);
}