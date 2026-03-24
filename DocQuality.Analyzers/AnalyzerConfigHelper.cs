using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DocQuality.Analyzers;

/// <summary>
///     Reads typed configuration values from <c>.editorconfig</c> via <see cref="AnalyzerConfigOptionsProvider"/>.
/// </summary>
internal static class AnalyzerConfigHelper
{
    public static bool GetBool(
        AnalyzerConfigOptionsProvider provider,
        SyntaxTree tree,
        string key,
        bool defaultValue)
    {
        return provider.GetOptions(tree).TryGetValue(key, out var value) &&
            bool.TryParse(value, out var result)
            ? result
            : defaultValue;
    }

    public static int GetInt(
        AnalyzerConfigOptionsProvider provider,
        SyntaxTree tree,
        string key,
        int defaultValue)
    {
        return provider.GetOptions(tree).TryGetValue(key, out var value) &&
            int.TryParse(value, out var result)
            ? result
            : defaultValue;
    }

    public static string? GetString(
        AnalyzerConfigOptionsProvider provider,
        SyntaxTree tree,
        string key)
    {
        return provider.GetOptions(tree).TryGetValue(key, out var value) ? value : null;
    }
}
