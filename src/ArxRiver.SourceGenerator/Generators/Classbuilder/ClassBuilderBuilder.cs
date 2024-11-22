using System.CodeDom.Compiler;
using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Configuration;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace ArxRiver.SourceGenerator.Generators.ClassBuilder;

internal static class ClassBuilderBuilder
{
    private static readonly string[] _systemUsings =
    [
        "System"
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
              {{GetAccessModifier(type)}} class {{type.Name.ToSafeClassName()}}Builder
              {
              """);
        writer.Indent++; // method level....

        writer.WriteLine();
        writer.WriteLine($"private readonly {type.Name.ToSafeClassName()} _instance = new {type.Name.ToSafeClassName()}();");

        // add class builder for properties with a user defined class type
        accessibleProperties.ToList().ForEach(property =>
        {
            if (property.Property.Type.TypeKind == TypeKind.Class && IsUserDefinedClass(property.Property.Type))
            {
                if (property.Property.Type.HasAttribute(GeneratorConstants.AutoClassBuilderAttributeName))
                {
                    writer.WriteLine($"private readonly {property.Name.ToSafeClassName()}Builder _{property.Name.ToCamelCase()}Builder = {property.Name.ToSafeClassName()}Builder.Create();");
                }
            }
        });

        writer.WriteLine($"private Func<{type.Name.ToSafeClassName()}, bool>? _validationRule = null;");
        writer.WriteLine();

        // Add Constructors
        writer.WriteLine($"private {type.Name.ToSafeClassName()}Builder() {{}}");
        writer.WriteLine();

        // static record to return a new instance of the builder
        writer.WriteLine($"public static {type.Name.ToSafeClassName()}Builder Create() => new {type.Name.ToSafeClassName()}Builder();");
        writer.WriteLine();

        // add Validation Rule....
        writer.WriteLine($"public {type.Name.ToSafeClassName()}Builder WithValidationRule(Func<{type.Name.ToSafeClassName()}, bool>? validationRule)");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine("_validationRule = validationRule;");
        writer.WriteLine("return this;");
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();

        accessibleProperties.ToList().ForEach(property =>
        {
            if (property.Property.Type.TypeKind == TypeKind.Class && IsUserDefinedClass(property.Property.Type))
            {
                if (property.Property.Type.HasAttribute(GeneratorConstants.AutoClassBuilderAttributeName))
                {
                    writer.WriteLine($"public {type.Name.ToSafeClassName()}Builder With{property.Name}(Action<{property.Name.ToSafeClassName()}Builder> {property.Name.ToSafeClassName().ToCamelCase()}BuilderAction)");
                    writer.WriteLine("{");
                    writer.Indent++;
                    writer.WriteLine($"{property.Name.ToCamelCase()}BuilderAction(_{property.Name.ToCamelCase()}Builder);");
                    writer.WriteLine("return this;");
                    writer.Indent--;
                    writer.WriteLine("}");
                    writer.WriteLine();
                }
            }

            writer.WriteLine($"public {type.Name.ToSafeClassName()}Builder With{property.Name}({property.Property.Type.ToDisplayString()} {property.Name.ToCamelCase()})");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine($"_instance.{property.Name} = {property.Name.ToCamelCase()};");
            writer.WriteLine("return this;");
            writer.Indent--;
            writer.WriteLine("}");
            writer.WriteLine();
        });

        writer.WriteLine();
        writer.WriteLine($"public {type.Name.ToSafeClassName()} Build(Func<{type.Name.ToSafeClassName()}, bool>?  validationRule = null)");
        writer.WriteLine("{");
        writer.Indent++;
        accessibleProperties.ToList().ForEach(property =>
        {
            if (property.Property.Type.TypeKind == TypeKind.Class && IsUserDefinedClass(property.Property.Type))
            {
                if (property.Property.Type.HasAttribute(GeneratorConstants.AutoClassBuilderAttributeName))
                {
                    writer.WriteLine($"_instance.{property.Name.ToSafeClassName()} = _{property.Name.ToCamelCase()}Builder.Build();");
                }
            }
        });
        
        writer.WriteLine();
        writer.WriteLine("_validationRule = validationRule ?? _validationRule;");
        writer.WriteLine("if (_validationRule != null && !_validationRule(_instance))");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine($"throw new ArgumentException(\"Validation failed for {type.Name.ToSafeClassName()}.\");");
        writer.Indent--;
        writer.WriteLine("}");

        writer.WriteLine("return _instance;");
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();

        writer.Indent--; // end of class
        writer.WriteLine("}");
    }

    private static ImmutableArray<PropertySymbolModel> GetAccessibleProperties(ImmutableArray<PropertySymbolModel> properties)
    {
        var result = ImmutableArray.Create<PropertySymbolModel>();

        foreach (var property in properties.Where(property => !property.HasSkipClassBuilderAttribute()))
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


    private static bool IsUserDefinedClass(ITypeSymbol typeSymbol)
    {
        switch (typeSymbol)
        {
            case INamedTypeSymbol namedTypeSymbol when typeSymbol.IsValueType && typeSymbol.SpecialType != SpecialType.None:
                return false;
            case INamedTypeSymbol namedTypeSymbol:
            {
                if (typeSymbol.ContainingNamespace != null)
                {
                    var ns = typeSymbol.ContainingNamespace.ToDisplayString();
                    if (ns == "System" || ns.StartsWith("System."))
                        return false;
                }

                if (namedTypeSymbol.TypeKind == TypeKind.Array)
                    return false;

                return true;
            }
            default:
                return false;
        }
    }


    private static string GetAccessModifier(TypeSymbolModel classSymbol) =>
        classSymbol.TypeSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();
}