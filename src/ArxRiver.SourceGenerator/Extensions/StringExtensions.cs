using System.CodeDom.Compiler;
using System.Text.RegularExpressions;

namespace ArxRiver.SourceGenerator.Extensions;

internal static class StringExtensions
{
    // This code came from Humanize:
    // https://github.com/Humanizr/Humanizer/blob/7492f69c25be62c3be8cd435d9ccaa95a2ef20e9/src/Humanizer/InflectorExtensions.cs
    // Trying to reference the package in the source generator
    // just wasn't working, and the implementation is pretty small
    // so I basically copied it here.
    // Giving credit where credit is due.
    internal static string ToCamelCase(this string instance)
    {
        var word = Regex.Replace(instance, "(?:^|_| +)(.)", match => match.Groups[1].Value.ToUpper());
        return word.Length > 0 ? word.Substring(0, 1).ToLower() + word.Substring(1) : word;
    }

    /// <summary>
    /// remove the leading comma
    /// RemoveLeadingCharacters(",..Hello", ',', '.'); 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string RemoveLeadingCharacters(this string input, params char[] charactersToRemove)
        => string.IsNullOrEmpty(input)
            ? input
            : input.Trim().TrimStart(charactersToRemove);

    /// <summary>
    /// remove the trailing comma
    /// RemoveTrailingCharacters(",..Hello", ',', '.'); 
    /// </summary>
    /// <param name="input"></param>
    /// <param name="charactersToRemove"></param>
    /// <returns></returns>
    public static string RemoveTrailingCharacters(this string input, params char[] charactersToRemove)
        => string.IsNullOrEmpty(input)
            ? input
            : input.Trim().TrimEnd(charactersToRemove);


    public static string ToSafeClassName(this string value) => Regex.Replace(value, "[,<>]", "_");
}