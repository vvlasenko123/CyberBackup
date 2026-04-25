using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CyberBackup.Analyzer.Cors.Provider
{
    /// <summary>
    /// Исправление для AddCors
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddCorsCodeFixProvider)), Shared]
    public sealed class AddCorsCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create("CYBER001");

        /// <inheritdoc />
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc />
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            if (root is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics[0];
            var node = root.FindNode(diagnostic.Location.SourceSpan);

            var memberAccess = node as SimpleNameSyntax;
            if (memberAccess is null)
            {
                return;
            }

            // Вариант 1
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Заменить на AddCyberCors",
                    cancellationToken => ReplaceAsync(
                        context.Document,
                        root,
                        memberAccess,
                        "AddCyberCors",
                        cancellationToken),
                    nameof(AddCorsCodeFixProvider) + "_AddCyberCors"),
                diagnostic);

            // Вариант 2
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Заменить на AddCyberDefaultCors",
                    cancellationToken => ReplaceAsync(
                        context.Document,
                        root,
                        memberAccess,
                        "AddCyberDefaultCors",
                        cancellationToken),
                    nameof(AddCorsCodeFixProvider) + "_AddCyberDefaultCors"),
                diagnostic);
        }

        /// <summary>
        /// Замена метода
        /// </summary>
        private static Task<Document> ReplaceAsync(
            Document document,
            SyntaxNode root,
            SimpleNameSyntax node,
            string newMethodName,
            CancellationToken cancellationToken)
        {
            var newName = SyntaxFactory.IdentifierName(newMethodName)
                .WithTriviaFrom(node);

            var newRoot = root.ReplaceNode(node, newName);

            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}