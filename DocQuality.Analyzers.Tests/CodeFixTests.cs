using Microsoft.CodeAnalysis;

namespace DocQuality.Analyzers.Tests;

public class CodeFixTests
{
    [Fact]
    public async Task DOC001_FillsEmptySummary()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// </summary>
                public void DoWork() { }
            }
            """;

        const string fixedSource = """
            public class MyClass
            {
                /// <summary>TODO: document this</summary>
                public void DoWork() { }
            }
            """;

        var expected = AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
            .WithSeverity(DiagnosticSeverity.Warning)
            .WithSpan(3, 9, 4, 19)
            .WithArguments("DoWork");

        var test = CodeFixVerifier.CreateTest(source, fixedSource, expected);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DOC002_FillsEmptyParamTag()
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

        const string fixedSource = """
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <param name="value">TODO: document this</param>
                public void DoWork(int value) { }
            }
            """;

        var expected = AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc002)
            .WithSeverity(DiagnosticSeverity.Warning)
            .WithSpan(6, 9, 6, 37)
            .WithArguments("DoWork", "value");

        var test = CodeFixVerifier.CreateTest(source, fixedSource, expected);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DOC004_FillsEmptyReturnsTag()
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

        const string fixedSource = """
            public class MyClass
            {
                /// <summary>
                /// Gets a value.
                /// </summary>
                /// <returns>TODO: document this</returns>
                public int GetValue() => 42;
            }
            """;

        var expected = AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc004)
            .WithSeverity(DiagnosticSeverity.Warning)
            .WithSpan(6, 9, 6, 28)
            .WithArguments("GetValue");

        var test = CodeFixVerifier.CreateTest(source, fixedSource, expected);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DOC005_FillsEmptyExceptionTag()
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

        const string fixedSource = """
            using System;
            public class MyClass
            {
                /// <summary>
                /// Does work.
                /// </summary>
                /// <exception cref="ArgumentNullException">TODO: document this</exception>
                public void DoWork() { }
            }
            """;

        var expected = AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc005)
            .WithSeverity(DiagnosticSeverity.Warning)
            .WithSpan(7, 9, 7, 61)
            .WithArguments("DoWork");

        var test = CodeFixVerifier.CreateTest(source, fixedSource, expected);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
