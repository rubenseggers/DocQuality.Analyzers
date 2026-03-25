using Microsoft.CodeAnalysis;

namespace DocQuality.Analyzers.Tests;

public class DOC003Tests
{
    [Fact]
    public async Task MissingParamTag_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                public void DoWork(int value) { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc003)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(6, 24, 6, 33)
                .WithArguments("DoWork", "value"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task OneParamDocumentedOneNot_ReportsForMissing()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <param name="a">First.</param>
                public void DoWork(int a, int b) { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc003)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(7, 31, 7, 36)
                .WithArguments("DoWork", "b"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AllParamsDocumented_NoDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <param name="a">First.</param>
                /// <param name="b">Second.</param>
                public void DoWork(int a, int b) { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NoParameters_NoDiagnostic()
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
    public async Task ConstructorMissingParamTag_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Creates instance.
                /// </summary>
                public MyClass(int value) { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc003)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(6, 20, 6, 29)
                .WithArguments("MyClass", "value"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task InheritDoc_NoDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <inheritdoc />
                public void DoWork(int value, string name) { }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RecordMissingParamTag_ReportsDiagnostic()
    {
        const string source = """
            /// <summary>A record.</summary>
            public record MyRecord(string Name);
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc003)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(2, 24, 2, 35)
                .WithArguments("MyRecord", "Name"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RecordAllParamsDocumented_NoDiagnostic()
    {
        const string source = """
            /// <summary>A record.</summary>
            /// <param name="Name">The name.</param>
            public record MyRecord(string Name);
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DelegateMissingParamTag_ReportsDiagnostic()
    {
        const string source = """
            /// <summary>A delegate.</summary>
            public delegate void MyDelegate(int value);
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc003)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(2, 33, 2, 42)
                .WithArguments("MyDelegate", "value"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task IndexerMissingParamTag_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>Gets an item.</summary>
                public int this[int index] => index;
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc003)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(4, 21, 4, 30)
                .WithArguments("this", "index"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
