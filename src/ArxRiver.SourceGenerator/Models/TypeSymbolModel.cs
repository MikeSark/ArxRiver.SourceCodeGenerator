#nullable enable
using ArxRiver;
using Microsoft.CodeAnalysis;

namespace ArxRiver.SourceGenerator.Models;

internal record TypeSymbolModel(
    INamedTypeSymbol TypeSymbol,
    string? ContainingNamespace,
    string Name,
    string GenericParameters,
    string FullyQualifiedName,
    string Constraints,
    bool IsValueType,
    bool IsPartial,
    EquatableArray<PropertySymbolModel> AccessibleProperties);