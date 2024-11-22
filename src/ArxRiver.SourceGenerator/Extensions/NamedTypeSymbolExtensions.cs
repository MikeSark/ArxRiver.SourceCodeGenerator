using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Configuration;
using ArxRiver.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArxRiver.SourceGenerator.Extensions;

internal static class NamedTypeSymbolExtensions
{
    internal static string GetGenericParameters(this INamedTypeSymbol symbol) => symbol.TypeParameters.Length > 0
        ? $"<{string.Join(", ", symbol.TypeParameters.Select(t => t.Name))}>"
        : string.Empty;

    internal static EquatableArray<PropertySymbolModel> GetAccessibleProperties(this INamedTypeSymbol self)
    {
        var targetType = self;
        var accessiblePropertiesBuilder = ImmutableArray.CreateBuilder<PropertySymbolModel>();

        while (targetType is not null)
        {
            accessiblePropertiesBuilder
                .AddRange(targetType.GetMembers().OfType<IPropertySymbol>()
                              .Where(p => !p.IsIndexer && p.GetMethod is not null &&
                                          p.GetMethod.DeclaredAccessibility == Accessibility.Public)
                              .Select(p =>
                                          new PropertySymbolModel(p, p.Name, p.ContainingSymbol.ContainingNamespace.ContainingNamespace.ToDisplayString(),
                                                                  p.Type.GetFullyQualifiedName(),
                                                                  p.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == GeneratorConstants.ExcludeAttributeFullPath))));
            targetType = targetType.BaseType;
        }

        return accessiblePropertiesBuilder.ToImmutable();
    }


    internal static string GetConstraints(this INamedTypeSymbol symbol)
    {
        if (symbol.TypeParameters.Length == 0)
        {
            return string.Empty;
        }
        else
        {
            var constraints = new List<string>(symbol.TypeParameters.Length);

            foreach (var parameter in symbol.TypeParameters)
            {
                var parameterConstraints = parameter.GetConstraints();

                if (parameterConstraints.Length > 0)
                {
                    constraints.Add(parameterConstraints);
                }
            }

            return constraints.Count > 0 ? string.Join(" ", constraints) : string.Empty;
        }
    }

    internal static bool IsPartial(this INamedTypeSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Any(syntaxRef =>
            {
                var syntax = syntaxRef.GetSyntax() as ClassDeclarationSyntax;
                return syntax?.Modifiers.Any(SyntaxKind.PartialKeyword) == true;
            });
    }


    private static string GetConstraints(this ITypeParameterSymbol symbol)
    {
        var constraints = new List<string>();

        // Based on what I've read here:
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#1425-type-parameter-constraints
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/where-generic-type-constraint
        // ...
        // Things like notnull and unmanaged should go first

        // According to CS0449, if any of these constraints exist: 
        // 'class', 'struct', 'unmanaged', 'notnull', and 'default'
        // they should not be duplicated.
        // Side note, I don't know how to find if the 'default'
        // constraint exists.
        if (symbol.HasUnmanagedTypeConstraint)
        {
            constraints.Add("unmanaged");
        }
        else if (symbol.HasNotNullConstraint)
        {
            constraints.Add("notnull");
        }
        // Then class constraint (HasReferenceTypeConstraint) or struct (HasValueTypeConstraint)
        else if (symbol.HasReferenceTypeConstraint)
        {
            constraints.Add(symbol.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "class?" : "class");
        }
        else if (symbol.HasValueTypeConstraint)
        {
            constraints.Add("struct");
        }

        // Then type constraints (classes first, then interfaces, then other generic type parameters)
        constraints.AddRange(symbol.ConstraintTypes.Where(_ => _.TypeKind == TypeKind.Class).Select(_ => _.GetFullyQualifiedName()));
        constraints.AddRange(symbol.ConstraintTypes.Where(_ => _.TypeKind == TypeKind.Interface).Select(_ => _.GetFullyQualifiedName()));
        constraints.AddRange(symbol.ConstraintTypes.Where(_ => _.TypeKind == TypeKind.TypeParameter).Select(_ => _.GetFullyQualifiedName()));

        // Then constructor constraint
        if (symbol.HasConstructorConstraint)
        {
            constraints.Add("new()");
        }

        return constraints.Count == 0 ? string.Empty : $"where {symbol.Name} : {string.Join(", ", constraints)}";
    }
}