using System.Text;

namespace ArxRiver.SourceGenerator.Extensions;

internal static class StringBuilderExtensions
{
    public static void AppendLine(this StringBuilder sb, int spaces, string value)
    {
        sb.AppendLine($"{new string(' ', spaces)}{value}");
    }

    public static void AppendLines(this StringBuilder sb, int spaces, IEnumerable<string> values, string postFix = "")
    {
        sb.AppendLines(values.Select(v => $"{new string(' ', spaces)}{v}"), postFix);
    }

    public static void AppendLines(this StringBuilder sb, IEnumerable<string> values, string postFix = "")
    {
        sb.AppendLine(string.Join($"{postFix}\r\n", values));
    }
    
    public static void AppendWithPrefixedTab(this StringBuilder sb, string value)
    {
        sb.AppendLine($"\t{value}");
    }

    public static void AppendWithSuffixedTab(this StringBuilder sb, string value)
    {
        sb.AppendLine($"{value}\t");
    }
}