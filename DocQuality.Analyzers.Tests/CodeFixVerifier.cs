using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace DocQuality.Analyzers.Tests;

internal static class CodeFixVerifier
{
    internal static CSharpCodeFixTest<DocAnalyzer, DocAnalyzerCodeFixProvider, DefaultVerifier> CreateTest(
        string source,
        string fixedSource,
        params DiagnosticResult[] expected)
    {
        var test = new CSharpCodeFixTest<DocAnalyzer, DocAnalyzerCodeFixProvider, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80
        };

        test.ExpectedDiagnostics.AddRange(expected);

        return test;
    }
}
