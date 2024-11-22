using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;

namespace ArxRiver.SourceGenerator.Generators.NLogger;

internal class BaseLogger
{
    private static readonly string[] _systemUsings =
    [
        "System",
        "NLog",
        "NLog.Config",
        "NLog.Layouts",
        "NLog.Targets"
    ];

    protected static void WriteNullableOption(IndentedTextWriter writer)
    {
        writer.WriteLine("#nullable enable");
        writer.WriteLine();
    }


    protected static void WriteUsings(IndentedTextWriter writer)
    {
        var usings = _systemUsings.ToList();
        writer.WriteLine(string.Join("\r\n", usings.Distinct().Select(u => $"using {u};")));
    }

    protected static void WriteNameSpaceAndTypeLocation(IndentedTextWriter writer, TypeSymbolModel type)
    {
        writer.WriteLines(
            $$"""
              
              namespace {{type.ContainingNamespace}};

              """);
    }

    protected static void WriteClassDefinitionBegin(IndentedTextWriter writer, TypeSymbolModel type)
    {
        writer.WriteLine($"public sealed class {type.Name}Builder");
        writer.WriteLine("{");
        writer.Indent++;
    }

    protected static void WriteClassDefinitionEnd(IndentedTextWriter writer)
    {
        writer.WriteLine();
        writer.Indent--;
        writer.WriteLine("}");
    }


    protected static void WriteConstructorBegin(IndentedTextWriter writer, TypeSymbolModel type)
    {
        writer.WriteLines(
            $$"""
              public {{type.Name}}Builder()
              {

              """);
    }


    protected static void WriteConstructorEnd(IndentedTextWriter writer)
    {
        writer.WriteLine("}");
    }
    
    
}