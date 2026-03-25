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

    [Fact]
    public async Task EmptySummaryOnClass_ReportsDiagnostic()
    {
        const string source = """
            /// <summary>
            /// </summary>
            public class MyClass { }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(1, 5, 2, 15)
                .WithArguments("MyClass"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NonEmptySummaryOnClass_NoDiagnostic()
    {
        const string source = """
            /// <summary>A test class.</summary>
            public class MyClass { }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptySummaryOnInterface_ReportsDiagnostic()
    {
        const string source = """
            /// <summary>
            /// </summary>
            public interface IMyInterface { }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(1, 5, 2, 15)
                .WithArguments("IMyInterface"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptySummaryOnStruct_ReportsDiagnostic()
    {
        const string source = """
            /// <summary>
            /// </summary>
            public struct MyStruct { }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(1, 5, 2, 15)
                .WithArguments("MyStruct"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptySummaryOnEnum_ReportsDiagnostic()
    {
        const string source = """
            /// <summary>
            /// </summary>
            public enum MyEnum { A, B }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(1, 5, 2, 15)
                .WithArguments("MyEnum"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptySummaryOnRecord_ReportsDiagnostic()
    {
        const string source = """
            /// <summary>
            /// </summary>
            public record MyRecord;
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(1, 5, 2, 15)
                .WithArguments("MyRecord"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptySummaryOnDelegate_ReportsDiagnostic()
    {
        const string source = """
            /// <summary>
            /// </summary>
            public delegate void MyDelegate();
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(1, 5, 2, 15)
                .WithArguments("MyDelegate"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptySummaryOnField_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// </summary>
                public int MyField;
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(3, 9, 4, 19)
                .WithArguments("MyField"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptySummaryOnEvent_ReportsDiagnostic()
    {
        const string source = """
            using System;
            public class MyClass
            {
                /// <summary>
                /// </summary>
                public event EventHandler MyEvent { add { } remove { } }
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(4, 9, 5, 19)
                .WithArguments("MyEvent"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptySummaryOnEventField_ReportsDiagnostic()
    {
        const string source = """
            using System;
            public class MyClass
            {
                /// <summary>
                /// </summary>
                public event EventHandler MyEvent;
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(4, 9, 5, 19)
                .WithArguments("MyEvent"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptySummaryOnIndexer_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// </summary>
                public int this[int index] => index;
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(3, 9, 4, 19)
                .WithArguments("this"),
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc003)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(5, 21, 5, 30)
                .WithArguments("this", "index"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptySummaryOnOperator_ReportsDiagnostic()
    {
        const string source = """
            public class MyClass
            {
                /// <summary>
                /// </summary>
                public static MyClass operator +(MyClass a, MyClass b) => a;
            }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(3, 9, 4, 19)
                .WithArguments("operator +"),
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc004)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(5, 19, 5, 26)
                .WithArguments("operator +"),
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc003)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(5, 38, 5, 47)
                .WithArguments("operator +", "a"),
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc003)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(5, 49, 5, 58)
                .WithArguments("operator +", "b"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PartialClass_WithEmptySummary_ReportsDiagnostic()
    {
        const string source = """
            /// <summary>
            /// </summary>
            public partial class MyClass { }
            """;

        var test = AnalyzerVerifier.CreateTest(source,
            AnalyzerVerifier.Diagnostic(DiagnosticDescriptors._doc001)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithSpan(1, 5, 2, 15)
                .WithArguments("MyClass"));
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task InternalClass_NoDiagnostic()
    {
        const string source = """
            /// <summary>
            /// </summary>
            internal class MyClass { }
            """;

        var test = AnalyzerVerifier.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
