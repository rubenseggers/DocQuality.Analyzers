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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeDelegate, SyntaxKind.DelegateDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeIndexer, SyntaxKind.IndexerDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeEvent, SyntaxKind.EventDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeEventField, SyntaxKind.EventFieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeField, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeOperator, SyntaxKind.OperatorDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeConversionOperator, SyntaxKind.ConversionOperatorDeclaration);
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

        var options = context.Options.AnalyzerConfigOptionsProvider;
        var tree = context.Node.SyntaxTree;

        var memberName = method.Identifier.Text;

        AnalyzeSummary(context, options, tree, docComment, memberName);
        AnalyzeParams(context, options, tree, docComment, memberName, method.ParameterList);
        AnalyzeReturns(context, options, tree, docComment, memberName, method.ReturnType);
        AnalyzeExceptions(context, options, tree, docComment, memberName);
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

        var options = context.Options.AnalyzerConfigOptionsProvider;
        var tree = context.Node.SyntaxTree;

        var memberName = constructor.Identifier.Text;

        AnalyzeSummary(context, options, tree, docComment, memberName);
        AnalyzeParams(context, options, tree, docComment, memberName, constructor.ParameterList);
        AnalyzeExceptions(context, options, tree, docComment, memberName);
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

        var options = context.Options.AnalyzerConfigOptionsProvider;
        var tree = context.Node.SyntaxTree;

        var memberName = property.Identifier.Text;

        AnalyzeSummary(context, options, tree, docComment, memberName);
        AnalyzeExceptions(context, options, tree, docComment, memberName);
    }

    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        var typeDecl = (BaseTypeDeclarationSyntax)context.Node;

        if (!typeDecl.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return;
        }

        var docComment = GetDocComment(typeDecl);

        if (docComment == null
            || HasInheritDoc(docComment))
        {
            return;
        }

        var options = context.Options.AnalyzerConfigOptionsProvider;
        var tree = context.Node.SyntaxTree;

        var memberName = typeDecl.Identifier.Text;

        AnalyzeSummary(context, options, tree, docComment, memberName);
        AnalyzeExceptions(context, options, tree, docComment, memberName);

        if (typeDecl is TypeDeclarationSyntax typeWithParams
            && typeWithParams.ParameterList != null)
        {
            AnalyzeParams(context, options, tree, docComment, memberName, typeWithParams.ParameterList);
        }
    }

    private static void AnalyzeDelegate(SyntaxNodeAnalysisContext context)
    {
        var delegateDecl = (DelegateDeclarationSyntax)context.Node;

        if (!delegateDecl.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return;
        }

        var docComment = GetDocComment(delegateDecl);

        if (docComment == null
            || HasInheritDoc(docComment))
        {
            return;
        }

        var options = context.Options.AnalyzerConfigOptionsProvider;
        var tree = context.Node.SyntaxTree;

        var memberName = delegateDecl.Identifier.Text;

        AnalyzeSummary(context, options, tree, docComment, memberName);
        AnalyzeParams(context, options, tree, docComment, memberName, delegateDecl.ParameterList);
        AnalyzeReturns(context, options, tree, docComment, memberName, delegateDecl.ReturnType);
        AnalyzeExceptions(context, options, tree, docComment, memberName);
    }

    private static void AnalyzeIndexer(SyntaxNodeAnalysisContext context)
    {
        var indexer = (IndexerDeclarationSyntax)context.Node;

        if (!IsPublicAndEligible(indexer.Modifiers, indexer))
        {
            return;
        }

        var docComment = GetDocComment(indexer);

        if (docComment == null
            || HasInheritDoc(docComment))
        {
            return;
        }

        var options = context.Options.AnalyzerConfigOptionsProvider;
        var tree = context.Node.SyntaxTree;

        var memberName = "this";

        AnalyzeSummary(context, options, tree, docComment, memberName);
        AnalyzeParams(context, options, tree, docComment, memberName, indexer.ParameterList);
        AnalyzeExceptions(context, options, tree, docComment, memberName);
    }

    private static void AnalyzeEvent(SyntaxNodeAnalysisContext context)
    {
        var eventDecl = (EventDeclarationSyntax)context.Node;

        if (!IsPublicAndEligible(eventDecl.Modifiers, eventDecl))
        {
            return;
        }

        var docComment = GetDocComment(eventDecl);

        if (docComment == null
            || HasInheritDoc(docComment))
        {
            return;
        }

        var options = context.Options.AnalyzerConfigOptionsProvider;
        var tree = context.Node.SyntaxTree;

        var memberName = eventDecl.Identifier.Text;

        AnalyzeSummary(context, options, tree, docComment, memberName);
        AnalyzeExceptions(context, options, tree, docComment, memberName);
    }

    private static void AnalyzeEventField(SyntaxNodeAnalysisContext context)
    {
        var eventField = (EventFieldDeclarationSyntax)context.Node;

        if (!eventField.Modifiers.Any(SyntaxKind.PublicKeyword)
            || eventField.Modifiers.Any(SyntaxKind.OverrideKeyword))
        {
            return;
        }

        var docComment = GetDocComment(eventField);

        if (docComment == null
            || HasInheritDoc(docComment))
        {
            return;
        }

        var options = context.Options.AnalyzerConfigOptionsProvider;
        var tree = context.Node.SyntaxTree;

        var memberName = eventField.Declaration.Variables.Count > 0
            ? eventField.Declaration.Variables[0].Identifier.Text
            : "event";

        AnalyzeSummary(context, options, tree, docComment, memberName);
        AnalyzeExceptions(context, options, tree, docComment, memberName);
    }

    private static void AnalyzeField(SyntaxNodeAnalysisContext context)
    {
        var field = (FieldDeclarationSyntax)context.Node;

        if (!field.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return;
        }

        var docComment = GetDocComment(field);

        if (docComment == null
            || HasInheritDoc(docComment))
        {
            return;
        }

        var options = context.Options.AnalyzerConfigOptionsProvider;
        var tree = context.Node.SyntaxTree;

        var memberName = field.Declaration.Variables.Count > 0
            ? field.Declaration.Variables[0].Identifier.Text
            : "field";

        AnalyzeSummary(context, options, tree, docComment, memberName);
    }

    private static void AnalyzeOperator(SyntaxNodeAnalysisContext context)
    {
        var operatorDecl = (OperatorDeclarationSyntax)context.Node;

        if (!IsPublicAndEligible(operatorDecl.Modifiers, operatorDecl))
        {
            return;
        }

        var docComment = GetDocComment(operatorDecl);

        if (docComment == null
            || HasInheritDoc(docComment))
        {
            return;
        }

        var options = context.Options.AnalyzerConfigOptionsProvider;
        var tree = context.Node.SyntaxTree;

        var memberName = "operator " + operatorDecl.OperatorToken.Text;

        AnalyzeSummary(context, options, tree, docComment, memberName);
        AnalyzeParams(context, options, tree, docComment, memberName, operatorDecl.ParameterList);
        AnalyzeReturns(context, options, tree, docComment, memberName, operatorDecl.ReturnType);
        AnalyzeExceptions(context, options, tree, docComment, memberName);
    }

    private static void AnalyzeConversionOperator(SyntaxNodeAnalysisContext context)
    {
        var conversionDecl = (ConversionOperatorDeclarationSyntax)context.Node;

        if (!IsPublicAndEligible(conversionDecl.Modifiers, conversionDecl))
        {
            return;
        }

        var docComment = GetDocComment(conversionDecl);

        if (docComment == null
            || HasInheritDoc(docComment))
        {
            return;
        }

        var options = context.Options.AnalyzerConfigOptionsProvider;
        var tree = context.Node.SyntaxTree;

        var memberName = conversionDecl.ImplicitOrExplicitKeyword.Text + " operator " + conversionDecl.Type;

        AnalyzeSummary(context, options, tree, docComment, memberName);
        AnalyzeParams(context, options, tree, docComment, memberName, conversionDecl.ParameterList);
        AnalyzeReturns(context, options, tree, docComment, memberName, conversionDecl.Type);
        AnalyzeExceptions(context, options, tree, docComment, memberName);
    }

    private static bool IsPublicAndEligible(SyntaxTokenList modifiers, MemberDeclarationSyntax member)
    {
        return modifiers.Any(SyntaxKind.PublicKeyword)
            && !modifiers.Any(SyntaxKind.OverrideKeyword)
            && !(member is MethodDeclarationSyntax
                && modifiers.Any(SyntaxKind.PartialKeyword));
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
        AnalyzerConfigOptionsProvider options,
        SyntaxTree tree,
        DocumentationCommentTriviaSyntax docComment,
        string memberName)
    {
        var requireSummary = AnalyzerConfigHelper.GetBool(
            options, tree, "dotnet_diagnostic.DOC001.require_summary", true);

        if (!requireSummary)
        {
            return;
        }

        if (IsExcludedGeneratedCode(options, tree, "DOC001"))
        {
            return;
        }

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
        AnalyzerConfigOptionsProvider options,
        SyntaxTree tree,
        DocumentationCommentTriviaSyntax docComment,
        string memberName,
        BaseParameterListSyntax? parameterList)
    {
        if (parameterList == null)
        {
            return;
        }

        var requireParam = AnalyzerConfigHelper.GetBool(
            options, tree, "dotnet_diagnostic.DOC002.require_param", true);
        var requireParamDescription = AnalyzerConfigHelper.GetBool(
            options, tree, "dotnet_diagnostic.DOC002.require_param_description", true);

        var doc002Excluded = IsExcludedGeneratedCode(options, tree, "DOC002");
        var doc003Excluded = IsExcludedGeneratedCode(options, tree, "DOC003");

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

                if (requireParamDescription && isEmpty && !doc002Excluded)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors._doc002, location!, memberName, paramName));
                }
            }
        }

        if (!requireParam || doc003Excluded)
        {
            return;
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
        AnalyzerConfigOptionsProvider options,
        SyntaxTree tree,
        DocumentationCommentTriviaSyntax docComment,
        string memberName,
        TypeSyntax returnType)
    {
        var requireReturns = AnalyzerConfigHelper.GetBool(
            options, tree, "dotnet_diagnostic.DOC004.require_returns", true);

        if (!requireReturns)
        {
            return;
        }

        if (IsVoidOrTaskLike(returnType))
        {
            return;
        }

        if (IsExcludedGeneratedCode(options, tree, "DOC004"))
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
        AnalyzerConfigOptionsProvider options,
        SyntaxTree tree,
        DocumentationCommentTriviaSyntax docComment,
        string memberName)
    {
        if (IsExcludedGeneratedCode(options, tree, "DOC005"))
        {
            return;
        }

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

    private static bool IsExcludedGeneratedCode(
        AnalyzerConfigOptionsProvider options,
        SyntaxTree tree,
        string diagnosticId)
    {
        var excludeGenerated = AnalyzerConfigHelper.GetBool(
            options, tree, $"dotnet_diagnostic.{diagnosticId}.exclude_generated_code", false);

        return excludeGenerated && IsGeneratedCode(tree);
    }

    private static bool IsGeneratedCode(SyntaxTree tree)
    {
        var filePath = tree.FilePath;

        if (!string.IsNullOrEmpty(filePath))
        {
            if (filePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
                || filePath.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase)
                || filePath.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        var root = tree.GetRoot();

        foreach (var attrList in root.DescendantNodes())
        {
            if (attrList is AttributeSyntax attr)
            {
                var name = attr.Name.ToString();

                if (name == "GeneratedCode"
                    || name == "GeneratedCodeAttribute"
                    || name.EndsWith(".GeneratedCode", StringComparison.Ordinal)
                    || name.EndsWith(".GeneratedCodeAttribute", StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
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
