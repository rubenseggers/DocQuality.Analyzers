using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DocQuality.Analyzers;

/// <summary>
///     Roslyn analyzer that validates XML documentation comment quality on public members.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DocAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors._doc001,
        DiagnosticDescriptors._doc002,
        DiagnosticDescriptors._doc003,
        DiagnosticDescriptors._doc004,
        DiagnosticDescriptors._doc005);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var method = (MethodDeclarationSyntax)context.Node;

        if (!IsPublicAndEligible(method.Modifiers, method))
        {
            return;
        }

        var docComment = GetDocComment(method);

        if (docComment == null
            || HasInheritDoc(docComment))
        {
            return;
        }

        var memberName = method.Identifier.Text;

        AnalyzeSummary(context, docComment, memberName);
        AnalyzeParams(context, docComment, memberName, method.ParameterList);
        AnalyzeReturns(context, docComment, memberName, method.ReturnType);
        AnalyzeExceptions(context, docComment, memberName);
    }

    private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
    {
        var constructor = (ConstructorDeclarationSyntax)context.Node;

        if (!IsPublicAndEligible(constructor.Modifiers, constructor))
        {
            return;
        }

        var docComment = GetDocComment(constructor);

        if (docComment == null
            || HasInheritDoc(docComment))
        {
            return;
        }

        var memberName = constructor.Identifier.Text;

        AnalyzeSummary(context, docComment, memberName);
        AnalyzeParams(context, docComment, memberName, constructor.ParameterList);
        AnalyzeExceptions(context, docComment, memberName);
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var property = (PropertyDeclarationSyntax)context.Node;

        if (!IsPublicAndEligible(property.Modifiers, property))
        {
            return;
        }

        var docComment = GetDocComment(property);

        if (docComment == null
            || HasInheritDoc(docComment))
        {
            return;
        }

        var memberName = property.Identifier.Text;

        AnalyzeSummary(context, docComment, memberName);
        AnalyzeExceptions(context, docComment, memberName);
    }

    private static bool IsPublicAndEligible(SyntaxTokenList modifiers, MemberDeclarationSyntax member)
    {
        return modifiers.Any(SyntaxKind.PublicKeyword)
            && !modifiers.Any(SyntaxKind.OverrideKeyword)
            && !modifiers.Any(SyntaxKind.PartialKeyword)
            && !(member is MethodDeclarationSyntax method
            && method.Modifiers.Any(SyntaxKind.PartialKeyword));
    }

    private static DocumentationCommentTriviaSyntax? GetDocComment(SyntaxNode node)
    {
        foreach (var trivia in node.GetLeadingTrivia())
        {
            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                && trivia.GetStructure() is DocumentationCommentTriviaSyntax doc)
            {
                return doc;
            }
        }

        return null;
    }

    private static bool HasInheritDoc(DocumentationCommentTriviaSyntax docComment)
    {
        foreach (var node in docComment.Content)
        {
            if (node is XmlEmptyElementSyntax empty
                && string.Equals(empty.Name.ToString(), "inheritdoc", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (node is XmlElementSyntax element
                && string.Equals(GetTagName(element), "inheritdoc", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void AnalyzeSummary(
        SyntaxNodeAnalysisContext context,
        DocumentationCommentTriviaSyntax docComment,
        string memberName)
    {
        foreach (var node in docComment.Content)
        {
            if (node is XmlElementSyntax element
                && GetTagName(element) == "summary")
            {
                if (IsElementContentEmpty(element))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors._doc001, element.GetLocation(), memberName));
                }

                return;
            }
        }
    }

    private static void AnalyzeParams(
        SyntaxNodeAnalysisContext context,
        DocumentationCommentTriviaSyntax docComment,
        string memberName,
        ParameterListSyntax? parameterList)
    {
        if (parameterList == null)
        {
            return;
        }

        var documentedParams = new HashSet<string>(StringComparer.Ordinal);

        foreach (var node in docComment.Content)
        {
            string? paramName = null;
            var isEmpty = false;
            Location? location = null;

            if (node is XmlElementSyntax element
                && GetTagName(element) == "param")
            {
                paramName = GetNameAttribute(element.StartTag.Attributes);
                isEmpty = IsElementContentEmpty(element);
                location = element.GetLocation();
            }
            else
            {
                if (node is XmlEmptyElementSyntax emptyElement
                    && emptyElement.Name.ToString() == "param")
                {
                    paramName = GetNameAttribute(emptyElement.Attributes);
                    isEmpty = true;
                    location = emptyElement.GetLocation();
                }
            }

            if (paramName != null)
            {
                documentedParams.Add(paramName);

                if (isEmpty)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors._doc002, location!, memberName, paramName));
                }
            }
        }

        foreach (var parameter in parameterList.Parameters)
        {
            var name = parameter.Identifier.Text;

            if (!documentedParams.Contains(name))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticDescriptors._doc003, parameter.GetLocation(), memberName, name));
            }
        }
    }

    private static void AnalyzeReturns(
        SyntaxNodeAnalysisContext context,
        DocumentationCommentTriviaSyntax docComment,
        string memberName,
        TypeSyntax returnType)
    {
        if (IsVoidOrTaskLike(returnType))
        {
            return;
        }

        var hasReturnsTag = false;

        foreach (var node in docComment.Content)
        {
            if (node is XmlElementSyntax element
                && GetTagName(element) == "returns")
            {
                hasReturnsTag = true;

                if (IsElementContentEmpty(element))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors._doc004, element.GetLocation(), memberName));
                }

                break;
            }

            if (node is XmlEmptyElementSyntax emptyElement
                && emptyElement.Name.ToString() == "returns")
            {
                hasReturnsTag = true;

                context.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticDescriptors._doc004, emptyElement.GetLocation(), memberName));

                break;
            }
        }

        if (!hasReturnsTag)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(DiagnosticDescriptors._doc004, returnType.GetLocation(), memberName));
        }
    }

    private static void AnalyzeExceptions(
        SyntaxNodeAnalysisContext context,
        DocumentationCommentTriviaSyntax docComment,
        string memberName)
    {
        foreach (var node in docComment.Content)
        {
            if (node is XmlElementSyntax element
                && GetTagName(element) == "exception")
            {
                if (IsElementContentEmpty(element))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors._doc005, element.GetLocation(), memberName));
                }
            }
            else
            {
                if (node is XmlEmptyElementSyntax emptyElement
                    && emptyElement.Name.ToString() == "exception")
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors._doc005, emptyElement.GetLocation(), memberName));
                }
            }
        }
    }

    private static string GetTagName(XmlElementSyntax element) => element.StartTag.Name.ToString();

    private static string? GetNameAttribute(SyntaxList<XmlAttributeSyntax> attributes)
    {
        foreach (var attr in attributes)
        {
            if (attr is XmlNameAttributeSyntax nameAttr)
            {
                return nameAttr.Identifier.Identifier.Text;
            }
        }

        return null;
    }

    private static bool IsElementContentEmpty(XmlElementSyntax element)
    {
        foreach (var content in element.Content)
        {
            if (content is XmlTextSyntax text)
            {
                foreach (var token in text.TextTokens)
                {
                    if (!string.IsNullOrWhiteSpace(token.Text))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsVoidOrTaskLike(TypeSyntax returnType)
    {
        var typeName = returnType switch
        {
            PredefinedTypeSyntax predefined => predefined.Keyword.Text,
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
            _ => null
        };

        return typeName is "void" or "Task" or "ValueTask";
    }
}
