using Microsoft.CodeAnalysis;

namespace ArxRiver.SourceGenerator.Models;

internal record PropertySymbolModel(IPropertySymbol Property, string Name, string NameSpace,  string TypeFullyQualifiedName, bool ExcludeProperty = false);