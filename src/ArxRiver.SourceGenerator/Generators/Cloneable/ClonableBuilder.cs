using System.CodeDom.Compiler;
using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace ArxRiver.SourceGenerator.Generators.Cloneable;

internal static class ClonableBuilder
{
    internal static void Build(IndentedTextWriter writer, TypeSymbolModel type, ImmutableArray<PropertySymbolModel> properties)
    {
        writer.WriteLine("#nullable enable");
        writer.WriteLine();

        if (type.ContainingNamespace is not null)
        {
            writer.WriteLines(
                $$"""
                  using {{type.FullyQualifiedName.Replace("global::", "").Replace($".{type.Name}", "")}};

                  namespace {{type.ContainingNamespace}};

                  """);
        }

        var clonableProperties = properties.Where(p => !p.HasSkipCloneableAttribute()).ToList();
        var propertyCloneLines = GetCloneableProperties(clonableProperties);

        writer.WriteLines(
            $$"""
              {{GetAccessModifier(type)}} partial class {{type.Name}}
              {
              """);
        writer.Indent++; // start of class

        // Simple Clone
        GenerateSimpleCloneMethod(writer, type, propertyCloneLines);

        // Simple Safe Clone
        GenerateSimpleCloneSafeMethod(writer, type, propertyCloneLines);

        // Simple Deep Clone
        GenerateDeepCloneMethod(writer, type, propertyCloneLines);

        // Safe Deep Clone
        GenerateDeepCloneSafeMethod(writer, type, propertyCloneLines);

        writer.Indent--; // end of class
        writer.WriteLine("}");
    }


    /// <summary>
    /// Generates a simple clone method for the specified type.
    /// </summary>
    /// <param name="writer">The <see cref="IndentedTextWriter"/> used to write the generated code.</param>
    /// <param name="type">The <see cref="TypeSymbolModel"/> representing the type for which the clone method is generated.</param>
    /// <param name="propertyCloneLines">A list of tuples containing the clone lines and their corresponding clone types.</param>
    private static void GenerateDeepCloneMethod(IndentedTextWriter writer, TypeSymbolModel type,
                                                List<(string cloneLine, bool isCloneable)> propertyCloneLines)
    {
        var cloneLines = propertyCloneLines.Select(cl => cl.isCloneable ? $"{cl.cloneLine}?.Clone()" : $"{cl.cloneLine}").ToArray();

        writer.WriteLines($"{GetSimpleDeepCloneMethodHeader(type)}");
        writer.WriteLine($"public {type.Name} DeepClone()");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine($"return new {type.Name}");
        writer.WriteLine("{");
        writer.Indent++;

        writer.WriteArray(cloneLines);
        writer.Indent--;
        writer.WriteLine("};");
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine("");
        writer.WriteLine("");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="type"></param>
    /// <param name="propertyCloneLines"></param>
    private static void GenerateDeepCloneSafeMethod(IndentedTextWriter writer, TypeSymbolModel type,
                                                    List<(string cloneLine, bool isCloneable)> propertyCloneLines)
    {
        var cloneLines = propertyCloneLines.Select(cl => cl.isCloneable ? $"{cl.cloneLine}?.SafeClone(referenceChain)" : $"{cl.cloneLine}").ToArray();

        writer.WriteLines($"{GetDeepCloneMethodHeader(type)}");
        writer.WriteLine($"public {type.Name} DeepCloneSafe(Stack<object>? referenceChain = null)");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLines(
            $"""
             if (referenceChain is not null && referenceChain.Contains(this))
                 return this;

             referenceChain ??= new Stack<object>();
             referenceChain.Push(this);

             var result = new {type.Name}
             """);
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteArray(cloneLines);
        writer.Indent--;
        writer.WriteLine("};");

        writer.WriteLines($"""

                           referenceChain.Pop();
                           return result;
                           """);


        writer.Indent--;
        writer.WriteLine("}");
    }

    /// <summary>
    /// Generates a method that creates a shallow copy of the specified type with circular reference checking.
    /// </summary>
    /// <param name="writer">The <see cref="IndentedTextWriter"/> used to write the generated code.</param>
    /// <param name="type">The <see cref="TypeSymbolModel"/> representing the type for which the clone method is generated.</param>
    /// <param name="propertyCloneLines">A list of tuples containing the clone lines and their corresponding clone types.</param>
    private static void GenerateSimpleCloneSafeMethod(IndentedTextWriter writer, TypeSymbolModel type,
                                                      List<(string cloneLine, bool isCloneable)> propertyCloneLines)
    {
        var cloneLines = propertyCloneLines.Select(cl => cl.cloneLine).ToArray();

        writer.WriteLines($"{GetCircularCloneHeader(type)}");
        writer.WriteLine($"public {type.Name} SafeClone(Stack<object>? referenceChain = null)");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLines(
            $"""
             if (referenceChain is not null && referenceChain.Contains(this))
                 return this;

             referenceChain ??= new Stack<object>();
             referenceChain.Push(this);

             var result = new {type.Name}
             """);
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteArray(cloneLines);
        writer.Indent--;
        writer.WriteLine("};");

        writer.WriteLines($"""

                           referenceChain.Pop();
                           return result;
                           """);

        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
    }


    /// <summary>
    /// Generates a simple clone method for the specified type.
    /// </summary>
    /// <param name="writer">The <see cref="IndentedTextWriter"/> used to write the generated code.</param>
    /// <param name="type">The <see cref="TypeSymbolModel"/> representing the type for which the clone method is generated.</param>
    /// <param name="propertyCloneLines">A list of tuples containing the clone lines and their corresponding clone types.</param>
    private static void GenerateSimpleCloneMethod(IndentedTextWriter writer, TypeSymbolModel type,
                                                  List<(string cloneLine, bool isCloneable)> propertyCloneLines)
    {
        var cloneLines = propertyCloneLines.Select(cl => cl.cloneLine).ToArray();

        writer.WriteLines($"{GetCloneMethodHeader(type)}");
        writer.WriteLine($"public {type.Name} Clone()");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine($"return new {type.Name}");
        writer.WriteLine("{");
        writer.Indent++;

        writer.WriteArray(cloneLines);
        writer.Indent--;
        writer.WriteLine("};");
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine("");
        writer.WriteLine("");
    }

    private static string GetAccessModifier(TypeSymbolModel classSymbol) =>
        classSymbol.TypeSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();


    private static List<(string cloneLine, bool isCloneable)>
        GetCloneableProperties(List<PropertySymbolModel> properties)
    {
        return properties
            .Select(property =>
                        new ValueTuple<string, bool>($"{property.Name} = this.{property.Name}",
                                                     (property.HasCloneAttribute() &&
                                                      property.Property.Type.IsGenericWithClassArgument() &&
                                                      property.Property.Type.IsCloneableType() &&
                                                      property.Property.IsPropertyClonable()))).ToList();
    }


    private static string GetCloneMethodHeader(TypeSymbolModel type)
    {
        return $"""
                /// <summary>
                /// Creates a simple clone of {type.Name} with NO circular reference check.
                /// </summary>
                """;
    }

    private static string GetCircularCloneHeader(TypeSymbolModel type)
    {
        return $"""
                /// <summary>
                /// Creates a simple cone of {type.Name} with circular reference check.
                /// </summary>
                """;
    }


    private static string GetDeepCloneMethodHeader(TypeSymbolModel type)
    {
        return $"""
                /// <summary>
                /// Creates a safe deep clone of {type.Name} with circular reference checking.
                /// </summary>
                """;
    }

    private static string GetSimpleDeepCloneMethodHeader(TypeSymbolModel type)
    {
        return $"""
                /// <summary>
                /// Creates a simple Deep Clone of {type.Name} with NO circular reference check.
                /// 
                /// </summary>
                """;
    }
}