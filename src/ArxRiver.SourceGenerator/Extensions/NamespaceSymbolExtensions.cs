using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArxRiver.SourceGenerator.Extensions;

internal static class NamespaceSymbolExtensions
{
    internal static bool Contains(this INamespaceSymbol symbol, INamespaceSymbol other) =>
        symbol.GetName().Contains(other.GetName());

    internal static string GetName(this INamespaceSymbol? symbol) =>
        symbol?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) ?? string.Empty;

}