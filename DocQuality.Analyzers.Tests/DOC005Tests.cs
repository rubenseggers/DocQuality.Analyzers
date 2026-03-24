using Microsoft.CodeAnalysis;

namespace DocQuality.Analyzers.Tests;

public class DOC005Tests
{
    [Fact]
    public async Task EmptyExceptionTag_ReportsDiagnostic()
    {
        const string source = """
            using System;
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <exception cref="ArgumentNullException"></exception>
                public void DoWork() { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc005)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(7, 9, 7, 61)
                .WithArguments("DoWork"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NonEmptyExceptionTag_NoDiagnostic()
    {
        const string source = """
            using System;
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <exception cref="ArgumentNullException">Thrown when null.</exception>
                public void DoWork() { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SelfClosingExceptionTag_ReportsDiagnostic()
    {
        const string source = """
            using System;
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <exception cref="ArgumentNullException" />
                public void DoWork() { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc005)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(7, 9, 7, 51)
                .WithArguments("DoWork"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task WhitespaceOnlyExceptionTag_ReportsDiagnostic()
    {
        const string source = """
            using System;
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <exception cref="ArgumentNullException">   </exception>
                public void DoWork() { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc005)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(7, 9, 7, 64)
                .WithArguments("DoWork"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MultipleExceptionTags_OnlyEmptyOnesReported()
    {
        const string source = """
            using System;
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <exception cref="ArgumentNullException">Thrown when null.</exception>
                /// <exception cref="InvalidOperationException"></exception>
                public void DoWork() { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc005)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 8, 65)
                .WithArguments("DoWork"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
