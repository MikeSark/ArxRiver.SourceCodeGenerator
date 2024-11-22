using ArxRiver.SourceGenerator.Configuration;
using ArxRiver.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace ArxRiver.SourceGenerator.Extensions;

public static class PropertySymbolModelExtensions
{
    internal static bool HasCloneableAttribute(this PropertySymbolModel property) =>
        PropertyHasAttribute(property.Property, GeneratorConstants.CloneableAttributeName);

    internal static bool HasCloneAttribute(this PropertySymbolModel property) =>
        PropertyHasAttribute(property.Property, GeneratorConstants.CloneAttributeName);
    
    internal static bool HasDeepCloneAttribute(this PropertySymbolModel property) =>
        PropertyHasAttribute(property.Property, GeneratorConstants.DeepCloneAttributeName);

    internal static bool HasExcludeAttribute(this PropertySymbolModel property) =>
        PropertyHasAttribute(property.Property, GeneratorConstants.ExcludeAttributeName);

    internal static bool HasSkipClassBuilderAttribute(this PropertySymbolModel property) =>
        PropertyHasAttribute(property.Property, GeneratorConstants.SkipClassBuilderAttributeName);

    internal static bool HasSkipCloneableAttribute(this PropertySymbolModel property) =>
        PropertyHasAttribute(property.Property, GeneratorConstants.SkipCloneableAttributeName);

    internal static bool HasIncludeInDeconstructAttribute(this PropertySymbolModel property) =>
        PropertyHasAttribute(property.Property, GeneratorConstants.IncludeInDeconstructAttributeName);

    internal static bool HasSkipFluentClassBuilderAttribute(this PropertySymbolModel property) =>
        PropertyHasAttribute(property.Property, GeneratorConstants.SkipFluentClassBuilderAttributeName);

    public static bool PropertyHasAttribute(IPropertySymbol propertySymbol, string attributeName)
    {
        return propertySymbol.GetAttributes()
            .Any(attributeData => attributeData.AttributeClass?.Name == attributeName);
    }
    
    
}