using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace DocQuality.Analyzers.Tests;

internal static class AnalyzerVerifier
{
    internal static CSharpAnalyzerTest<DocAnalyzer, DefaultVerifier> CreateTest(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<DocAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80
        };

        test.ExpectedDiagnostics.AddRange(expected);

        return test;
    }

    internal static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) 
        => CSharpAnalyzerVerifier<DocAnalyzer, DefaultVerifier>.Diagnostic(descriptor);
}
