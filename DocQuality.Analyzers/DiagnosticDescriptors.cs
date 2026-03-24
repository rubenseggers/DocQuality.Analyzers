using Microsoft.CodeAnalysis;

namespace DocQuality.Analyzers;

/// <summary>
///     Diagnostic descriptors for all DOC-series XML documentation diagnostics.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string _category = "Documentation";
    internal static readonly DiagnosticDescriptor _doc001 = new(
        "DOC001",
        "Summary tag is empty",
        "The <summary> tag on '{0}' is empty or whitespace-only",
        _category,
        DiagnosticSeverity.Warning,
        true,
        "A public member has a <summary> tag that contains no meaningful text.");
    internal static readonly DiagnosticDescriptor _doc002 = new(
        "DOC002",
        "Param tag is empty",
        "The <param name=\"{1}\"> tag on '{0}' is empty or whitespace-only",
        _category,
        DiagnosticSeverity.Warning,
        true,
        "A public member has a <param> tag that contains no meaningful text.");
    internal static readonly DiagnosticDescriptor _doc003 = new(
        "DOC003",
        "Missing param tag",
        "Parameter '{1}' on '{0}' has no corresponding <param> tag",
        _category,
        DiagnosticSeverity.Warning,
        true,
        "A public member has a parameter with no matching <param> tag in the XML doc comment.");
    internal static readonly DiagnosticDescriptor _doc004 = new(
        "DOC004",
        "Missing or empty returns tag",
        "'{0}' returns a value but has no <returns> tag or the tag is empty",
        _category,
        DiagnosticSeverity.Warning,
        true,
        "A public member returns a non-void value but is missing a <returns> tag or has an empty one.");
    internal static readonly DiagnosticDescriptor _doc005 = new(
        "DOC005",
        "Exception tag is empty",
        "The <exception> tag on '{0}' is empty or whitespace-only",
        _category,
        DiagnosticSeverity.Warning,
        true,
        "A public member has an <exception> tag that contains no meaningful text.");
}
