using Microsoft.CodeAnalysis;

namespace DocQuality.Analyzers.Tests;

public class DOC001Tests
{
    [Fact]
    public async Task EmptySummary_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// </summary>
                public void DoWork() { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(3, 9, 4, 19)
                .WithArguments("DoWork"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task WhitespaceOnlySummary_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                ///     
                /// </summary>
                public void DoWork() { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(3, 9, 5, 19)
                .WithArguments("DoWork"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NonEmptySummary_NoDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                public void DoWork() { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NoDocComment_NoDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                public void DoWork() { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task OverrideMethod_NoDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// </summary>
                public override string ToString() => "test";
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PrivateMethod_NoDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// </summary>
                private void DoWork() { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptySummaryOnConstructor_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// </summary>
                public MyClass() { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(3, 9, 4, 19)
                .WithArguments("MyClass"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptySummaryOnProperty_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// </summary>
                public int Value { get; set; }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(3, 9, 4, 19)
                .WithArguments("Value"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task InheritDoc_NoDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <inheritdoc />
                public void DoWork() { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
