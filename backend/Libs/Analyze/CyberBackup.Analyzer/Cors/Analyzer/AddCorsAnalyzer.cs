using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CyberBackup.Analyzer.Cors.Analyzer
{
    /// <summary>
    /// Анализатор, запрещающий использование AddCors
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class AddCorsAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Идентификатор диагностики
        /// </summary>
        public const string DiagnosticId = "CYBER001";

        /// <summary>
        /// Описание правила
        /// </summary>
        private static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                DiagnosticId,
                "Запрещено использовать AddCors",
                "Используйте AddCyberDefaultCors вместо AddCors",
                "Architecture",
                DiagnosticSeverity.Error,
                true);

        /// <summary>
        /// Список поддерживаемых диагностик
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <summary>
        /// Инициализация анализатора
        /// </summary>
        public override void Initialize(AnalysisContext context)
        {
            // Не анализируем сгенерированный код
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // Разрешаем параллельное выполнение
            context.EnableConcurrentExecution();

            // Регистрируем обработчик для вызовов методов
            context.RegisterSyntaxNodeAction(
                AnalyzeInvocation,
                Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
        }

        /// <summary>
        /// Анализ вызова метода
        /// </summary>
        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null)
            {
                return;
            }

            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
            {
                return;
            }

            if (memberAccess.Name.Identifier.Text != "AddCors")
            {
                return;
            }

            var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;
            if (symbol == null)
            {
                return;
            }

            if (symbol.ContainingType.ToDisplayString() !=
                "Microsoft.Extensions.DependencyInjection.CorsServiceCollectionExtensions")
            {
                return;
            }

            var assemblyName = context.Compilation.AssemblyName;
            if (assemblyName == "Security.Host")
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, memberAccess.Name.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}