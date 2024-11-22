using System.CodeDom.Compiler;
using System.Reflection;

namespace ArxRiver.SourceGenerator.Extensions;

internal static class IndentedTextWriterExtensions
{
    // This is fragile, but hopefully we can get a public readonly property for this private field
    // in the future.
    internal static string GetTabString(this IndentedTextWriter writer)
    {
        var tabStringField = typeof(IndentedTextWriter).GetField("_tabString", BindingFlags.NonPublic | BindingFlags.Instance);
        return tabStringField is not null ? (string)tabStringField.GetValue(writer)! : "\t";
    }

    internal static void WriteLines(this IndentedTextWriter writer, string content, string templateIndentation = "\t", int indentation = 0)
    {
        var tabString = writer.GetTabString();

        if (indentation > 0)
        {
            writer.Indent += indentation;
        }

        foreach (var line in content.Split(new[] { writer.NewLine }, StringSplitOptions.None))
        {
            var contentLine = line;

            if (templateIndentation != tabString)
            {
                var foundTemplateIndentationCount = 0;

                while (contentLine.StartsWith(templateIndentation, StringComparison.InvariantCultureIgnoreCase))
                {
                    contentLine = contentLine.Substring(templateIndentation.Length);
                    foundTemplateIndentationCount++;
                }

                for (var i = 0; i < foundTemplateIndentationCount; i++)
                {
                    contentLine = contentLine.Insert(0, tabString);
                }
            }

            writer.WriteLine(contentLine);
        }

        if (indentation > 0)
        {
            writer.Indent -= indentation;
        }
    }


    public static void WriteArray(this IndentedTextWriter writer, string[]? array)
    {
        if (array == null || array.Length == 0)
            return;

        for (var i = 0; i < array.Length; i++)
        {
            writer.WriteLine((i == array.Length - 1)
                                 ? array[i]
                                 : array[i] + ",");
        }
    }
}