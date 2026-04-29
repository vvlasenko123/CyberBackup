using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using CyberBackup.Analyzer.Cors.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CyberBackup.Analyzer.Cors.Provider
{
    /// <summary>
    /// CodeFix для замены UseCors на UseCyberCors
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCorsCodeFixProvider)), Shared]
    public sealed class UseCorsCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds 
            => ImmutableArray.Create(UseCorsAnalyzer.DiagnosticId);

        /// <inheritdoc />
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc />
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            if (root is null)
            {
                return;
            }

            var node = root.FindNode(diagnostic.Location.SourceSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Заменить на UseCyberCors",
                    c => ReplaceAsync(context.Document, node, c),
                    "ReplaceWithUseCyberCors"),
                diagnostic);
        }

        /// <summary>
        /// Замена метода
        /// </summary>
        private static async Task<Document> ReplaceAsync(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            if (root is null)
            {
                return document;
            }

            var memberAccess = node.Parent as MemberAccessExpressionSyntax;
            if (memberAccess is null)
            {
                return document;
            }

            var newName = SyntaxFactory.IdentifierName("UseCyberCors")
                .WithTriviaFrom(memberAccess.Name);

            var newMemberAccess = memberAccess.WithName(newName);

            var newRoot = root.ReplaceNode(memberAccess, newMemberAccess);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}