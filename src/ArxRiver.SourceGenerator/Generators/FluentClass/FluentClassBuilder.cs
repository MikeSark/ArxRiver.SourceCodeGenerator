using System.CodeDom.Compiler;
using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace ArxRiver.SourceGenerator.Generators.FluentClass;

internal static class FluentClassBuilder
{
    private static readonly string[] _systemUsings =
    [
        "System",
        "System.Collections",
        "System.Collections.Generic",
        "System.Linq",
        "System.Linq.Expressions"
    ];


    internal static void Build(IndentedTextWriter writer, TypeSymbolModel type, ImmutableArray<PropertySymbolModel> properties)
    {
        var accessibleProperties = GetAccessibleProperties(properties);

        writer.WriteLine("#nullable enable");
        writer.WriteLine();

        // Write usings statement
        var usings = _systemUsings.ToList();
        writer.WriteLine(string.Join("\r\n", usings.Distinct().Select(u => $"using {u};")));

        // add class using and namespace...
        if (type.ContainingNamespace is not null)
        {
            writer.WriteLines(
                $$"""
                  using {{type.FullyQualifiedName.Replace("global::", "").Replace($".{type.Name}", "")}};

                  namespace {{type.ContainingNamespace}};

                  """);
        }

        writer.WriteLines(
            $$"""
              {{GetAccessModifier(type)}} partial class {{type.Name.ToSafeClassName()}}
              {
              """);
        writer.Indent++; // method level....

        writer.WriteLine();
        writer.WriteLine($"private readonly Func<{type.Name.ToSafeClassName()}, bool>? _validationRule;");
        writer.WriteLine();

        // Add Constructors
        writer.WriteLine($"public {type.Name.ToSafeClassName()}() {{}}");
        writer.WriteLine();
        writer.WriteLine($"public {type.Name.ToSafeClassName()}(Func<{type.Name.ToSafeClassName()}, bool>? validationRule = null)");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine("_validationRule = validationRule;");
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();

        accessibleProperties.ToList().ForEach(property =>
        {
            writer.WriteLine($"public {type.Name.ToSafeClassName()} With{property.Name}({property.Property.Type} {property.Name.ToCamelCase()})");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine($"{property.Name} = {property.Name.ToCamelCase()};");
            writer.WriteLine("return this;");
            writer.Indent--;
            writer.WriteLine("}");
            writer.WriteLine();
        });

        writer.WriteLine();
        writer.WriteLine($"public bool Validate()");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine("return _validationRule?.Invoke(this) ?? true;");
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
        writer.Indent--; // end of class
        writer.WriteLine("}");
    }

    private static ImmutableArray<PropertySymbolModel> GetAccessibleProperties(ImmutableArray<PropertySymbolModel> properties)
    {
        var result = ImmutableArray.Create<PropertySymbolModel>();

        foreach (var property in properties.Where(property => !property.HasSkipFluentClassBuilderAttribute()))
        {
            if (property.Property.SetMethod is not null &&
                !property.Property.IsReadOnly &&
                property.Property.SetMethod.DeclaredAccessibility == Accessibility.Public)
            {
                result = result.Add(property);
            }
        }

        return result;
    }

    private static string GetAccessModifier(TypeSymbolModel classSymbol) =>
        classSymbol.TypeSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();
}