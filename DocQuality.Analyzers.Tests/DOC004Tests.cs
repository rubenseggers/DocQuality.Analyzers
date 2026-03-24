using Microsoft.CodeAnalysis;

namespace DocQuality.Analyzers.Tests;

public class DOC004Tests
{
    [Fact]
    public async Task MissingReturnsTag_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Gets a value.
                /// </summary>
                public int GetValue() => 42;
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc004)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(6, 12, 6, 15)
                .WithArguments("GetValue"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptyReturnsTag_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Gets a value.
                /// </summary>
                /// <returns></returns>
                public int GetValue() => 42;
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc004)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(6, 9, 6, 28)
                .WithArguments("GetValue"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NonEmptyReturnsTag_NoDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Gets a value.
                /// </summary>
                /// <returns>The value.</returns>
                public int GetValue() => 42;
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task VoidMethod_NoDiagnostic()
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
    public async Task TaskMethod_NoDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                public Task DoWorkAsync() => Task.CompletedTask;
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValueTaskMethod_NoDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                public ValueTask DoWorkAsync() => default;
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GenericTaskMethod_RequiresReturns()
    {
        const string source = """
            using System.Threading.Tasks;
            public class MyClass
            {
                /// <summary>
                /// Gets a value.
                /// </summary>
                public Task<int> GetValueAsync() => Task.FromResult(42);
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc004)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(7, 12, 7, 21)
                .WithArguments("GetValueAsync"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SelfClosingReturnsTag_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// Gets a value.
                /// </summary>
                /// <returns />
                public int GetValue() => 42;
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc004)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(6, 9, 6, 20)
                .WithArguments("GetValue"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task InheritDoc_NoDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <inheritdoc />
                public int GetValue() => 42;
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
