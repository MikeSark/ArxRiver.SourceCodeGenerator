using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace ArxRiver.SourceGenerator.Extensions;

internal static class ClassbuilderHelpers
{
    public static void WriteUsings(this IndentedTextWriter writer, string[] usings) =>
        writer.WriteLine(string.Join("\r\n", usings.Distinct().Select(u => $"using {u};")));

    public static void WriteEndBraket(IndentedTextWriter writer)
    {
        writer.WriteLine("}");
    }

    public static void WriteStartBraket(IndentedTextWriter writer)
    {
        writer.WriteLine("{");
    }

    public static void WriteNullableOptions(this IndentedTextWriter writer)
    {
        writer.WriteLine("#nullable enable");
        writer.WriteLine();
    }
}