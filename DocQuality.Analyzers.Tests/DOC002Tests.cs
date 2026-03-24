using Microsoft.CodeAnalysis;

namespace DocQuality.Analyzers.Tests;
public class DOC002Tests
{
    [Fact]
    public async Task EmptyParamTag_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <param name="value"></param>
                public void DoWork(int value) { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc002)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(6, 9, 6, 37)
                .WithArguments("DoWork", "value"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task WhitespaceOnlyParamTag_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <param name="value">   </param>
                public void DoWork(int value) { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc002)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(6, 9, 6, 40)
                .WithArguments("DoWork", "value"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NonEmptyParamTag_NoDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <param name="value">The value to use.</param>
                public void DoWork(int value) { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SelfClosingParamTag_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <param name="value" />
                public void DoWork(int value) { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc002)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(6, 9, 6, 31)
                .WithArguments("DoWork", "value"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
