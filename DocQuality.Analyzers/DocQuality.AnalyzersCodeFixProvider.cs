using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocQuality.Analyzers;

/// <summary>
///     Provides code fixes for DOC001–DOC005 diagnostics by inserting placeholder documentation text.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocAnalyzerCodeFixProvider))]
[Shared]
public sealed class DocAnalyzerCodeFixProvider : CodeFixProvider
{
    private const string _placeholder = "TODO: document this";

    public override ImmutableArray<string> FixableDiagnosticIds { get; }
        = ImmutableArray.Create("DOC001", "DOC002", "DOC003", "DOC004", "DOC005");

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var node = root.FindNode(diagnosticSpan, true, true);

            switch (diagnostic.Id)
            {
                case "DOC001":
                    RegisterFillTag(context, diagnostic, node, "summary");

                    break;

                case "DOC002":
                    RegisterFillTag(context, diagnostic, node, "param");

                    break;

                case "DOC003":
                    RegisterAddParamTag(context, diagnostic, node);

                    break;

                case "DOC004":
                    RegisterAddOrFillReturnsTag(context, diagnostic, node);

                    break;

                case "DOC005":
                    RegisterFillTag(context, diagnostic, node, "exception");

                    break;
            }
        }
    }

    private static void RegisterFillTag(CodeFixContext context, Diagnostic diagnostic, SyntaxNode node, string tagName)
    {
        var element = node.AncestorsAndSelf().OfType<XmlElementSyntax>().FirstOrDefault()
                      ?? node.AncestorsAndSelf().OfType<XmlEmptyElementSyntax>().FirstOrDefault()?.Parent as
                          XmlElementSyntax;

        if (element == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                $"Fill empty <{tagName}> tag",
                ct => FillTagAsync(context.Document, element, ct),
                diagnostic.Id),
            diagnostic);
    }

    private static async Task<Document> FillTagAsync(
        Document document,
        XmlElementSyntax element,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var newContent = SyntaxFactory.XmlText(_placeholder);
        var newElement = element.WithContent(SyntaxFactory.SingletonList<XmlNodeSyntax>(newContent));
        var newRoot = root.ReplaceNode(element, newElement);

        return document.WithSyntaxRoot(newRoot);
    }

    private static void RegisterAddParamTag(CodeFixContext context, Diagnostic diagnostic, SyntaxNode node)
    {
        var parameter = node.AncestorsAndSelf().OfType<ParameterSyntax>().FirstOrDefault();

        if (parameter == null)
        {
            return;
        }

        var paramName = parameter.Identifier.Text;

        context.RegisterCodeFix(
            CodeAction.Create(
                $"Add <param name=\"{paramName}\"> tag",
                ct => AddParamTagAsync(context.Document, parameter, ct),
                diagnostic.Id),
            diagnostic);
    }

    private static async Task<Document> AddParamTagAsync(
        Document document,
        ParameterSyntax parameter,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var member = parameter.FirstAncestorOrSelf<MemberDeclarationSyntax>();

        if (member == null)
        {
            return document;
        }

        var docComment = member
                         .GetLeadingTrivia()
                         .Select(t => t.GetStructure())
                         .OfType<DocumentationCommentTriviaSyntax>()
                         .FirstOrDefault();

        if (docComment == null)
        {
            return document;
        }

        var paramName = parameter.Identifier.Text;

        var nameAttribute = SyntaxFactory.XmlNameAttribute(
            SyntaxFactory.XmlName("name"),
            SyntaxFactory.Token(SyntaxKind.DoubleQuoteToken),
            SyntaxFactory.IdentifierName(paramName),
            SyntaxFactory.Token(SyntaxKind.DoubleQuoteToken));

        var paramTag = SyntaxFactory.XmlElement(
            SyntaxFactory.XmlElementStartTag(
                SyntaxFactory.XmlName("param"),
                SyntaxFactory.SingletonList<XmlAttributeSyntax>(nameAttribute)),
            SyntaxFactory.SingletonList<XmlNodeSyntax>(SyntaxFactory.XmlText(_placeholder)),
            SyntaxFactory.XmlElementEndTag(SyntaxFactory.XmlName("param")));

        var newLine = SyntaxFactory.XmlText(
            SyntaxFactory.TokenList(
                SyntaxFactory.XmlTextNewLine(SyntaxFactory.TriviaList(), "\r\n", "\r\n", SyntaxFactory.TriviaList()),
                SyntaxFactory.XmlTextLiteral(
                    SyntaxFactory.TriviaList(SyntaxFactory.DocumentationCommentExterior("/// ")),
                    " ",
                    " ",
                    SyntaxFactory.TriviaList())));

        var newContent = docComment.Content.Add(newLine).Add(paramTag);
        var newDocComment = docComment.WithContent(newContent);
        var newRoot = root.ReplaceNode(docComment, newDocComment);

        return document.WithSyntaxRoot(newRoot);
    }

    private static void RegisterAddOrFillReturnsTag(CodeFixContext context, Diagnostic diagnostic, SyntaxNode node)
    {
        var element = node.AncestorsAndSelf().OfType<XmlElementSyntax>().FirstOrDefault();

        if (element != null
            && GetTagName(element) == "returns")
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Fill empty <returns> tag",
                    ct => FillTagAsync(context.Document, element, ct),
                    diagnostic.Id),
                diagnostic);
        }
        else
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Add <returns> tag",
                    ct => AddReturnsTagAsync(context.Document, node, ct),
                    diagnostic.Id),
                diagnostic);
        }
    }

    private static async Task<Document> AddReturnsTagAsync(
        Document document,
        SyntaxNode node,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var member = node.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().FirstOrDefault();

        if (member == null)
        {
            return document;
        }

        var docComment = member
                         .GetLeadingTrivia()
                         .Select(t => t.GetStructure())
                         .OfType<DocumentationCommentTriviaSyntax>()
                         .FirstOrDefault();

        if (docComment == null)
        {
            return document;
        }

        var returnsTag = SyntaxFactory.XmlElement(
            SyntaxFactory.XmlElementStartTag(SyntaxFactory.XmlName("returns")),
            SyntaxFactory.SingletonList<XmlNodeSyntax>(SyntaxFactory.XmlText(_placeholder)),
            SyntaxFactory.XmlElementEndTag(SyntaxFactory.XmlName("returns")));

        var newLine = SyntaxFactory.XmlText(
            SyntaxFactory.TokenList(
                SyntaxFactory.XmlTextNewLine(SyntaxFactory.TriviaList(), "\r\n", "\r\n", SyntaxFactory.TriviaList()),
                SyntaxFactory.XmlTextLiteral(
                    SyntaxFactory.TriviaList(SyntaxFactory.DocumentationCommentExterior("/// ")),
                    " ",
                    " ",
                    SyntaxFactory.TriviaList())));

        var newContent = docComment.Content.Add(newLine).Add(returnsTag);
        var newDocComment = docComment.WithContent(newContent);
        var newRoot = root.ReplaceNode(docComment, newDocComment);

        return document.WithSyntaxRoot(newRoot);
    }

    private static string GetTagName(XmlElementSyntax element) => element.StartTag.Name.ToString();
}
