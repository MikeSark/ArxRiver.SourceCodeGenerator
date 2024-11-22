using System.CodeDom.Compiler;
using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;

namespace ArxRiver.SourceGenerator.Generators.Deconstruct;

internal static class AutoDeconstructBuilder
{
    private static readonly string[] _systemUsings =
    [
        "System"
    ];

    internal static void Build(IndentedTextWriter writer, TypeSymbolModel type, ImmutableArray<PropertySymbolModel> properties)
    {
        var deConstructedProperties = properties.Where(p => p.HasIncludeInDeconstructAttribute()).ToList();
        if (deConstructedProperties.Count == 0) return;

        writer.WriteLine("#nullable enable");
        writer.WriteLine();

        // Write usings statement
        var usings = _systemUsings.ToList();
        writer.WriteLine(string.Join("\r\n", usings.Distinct().Select(u => $"using {u};")));

        if (type.ContainingNamespace is not null)
        {
            writer.WriteLines(
                $$"""
                  using {{type.FullyQualifiedName.Replace("global::", "").Replace($".{type.Name}", "")}};

                  namespace {{type.ContainingNamespace}}
                  {
                  """);
            writer.Indent++;
        }

        writer.WriteLines(
            $$"""
              public static partial class {{type.Name}}Extensions
              {
              """);
        writer.Indent++;

        // todo: in this stage we should optimize.

        var outParameters = string.Join(", ", deConstructedProperties.Select(p => $"out {p.TypeFullyQualifiedName} {p.Name.ToCamelCase()}"));

        var namingContext = new VariableNamingContext(deConstructedProperties.Select(p => p.Name.ToCamelCase()).ToImmutableArray());

        writer.WriteLine(
            $$"""public static void Deconstruct{{type.GenericParameters}}(this {{type.Name}} {{namingContext["input"]}}, {{outParameters}})""");

        var constraints = type.Constraints;

        if (constraints.Length > 0)
        {
            writer.Indent++;
            writer.WriteLine(constraints);
            writer.Indent--;
        }

        writer.WriteLine("{");
        writer.Indent++;

        if (!type.IsValueType)
        {
            writer.WriteLine($"global::System.ArgumentNullException.ThrowIfNull({namingContext["input"]});");
        }

        if (properties.Length == 1 && !properties[0].ExcludeProperty)
        {
            writer.WriteLine($"@{properties[0].Name.ToCamelCase()} = {namingContext["input"]}.{properties[0].Name};");
        }
        else
        {
            writer.WriteLine($"({string.Join(", ", deConstructedProperties.Select(p => $"{p.Name.ToCamelCase()}"))}) =");
            writer.Indent++;
            writer.WriteLine($"({string.Join(", ", deConstructedProperties.Select(p => $"{namingContext["input"]}.{p.Name}"))});");
            writer.Indent--;
        }

        writer.Indent--;
        writer.WriteLine("}");
        writer.Indent--;
        writer.WriteLine("}");

        if (type.ContainingNamespace is not null)
        {
            writer.Indent--;
            writer.WriteLine("}");
        }
    }
}