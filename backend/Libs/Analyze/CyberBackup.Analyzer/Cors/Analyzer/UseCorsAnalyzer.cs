using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CyberBackup.Analyzer.Cors.Analyzer
{
    /// <summary>
    /// Анализатор, запрещающий использование UseCors
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class UseCorsAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Идентификатор диагностики
        /// </summary>
        public const string DiagnosticId = "CYBER002";

        /// <summary>
        /// Описание правила
        /// </summary>
        private static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                DiagnosticId,
                "Запрещено использовать UseCors",
                "Используйте UseCyberCors вместо UseCors",
                "Architecture",
                DiagnosticSeverity.Error,
                true);

        /// <summary>
        /// Список поддерживаемых диагностик
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        /// <summary>
        /// Инициализация анализатора
        /// </summary>
        public override void Initialize(AnalysisContext context)
        {
            // Не анализируем сгенерированный код
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // Разрешаем параллельное выполнение
            context.EnableConcurrentExecution();

            // Регистрируем обработчик вызовов методов
            context.RegisterSyntaxNodeAction(
                AnalyzeInvocation,
                Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
        }

        /// <summary>
        /// Анализ вызова метода
        /// </summary>
        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            // Приведение к InvocationExpression
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null)
            {
                return;
            }

            // Проверка вызова через точку (app.UseCors)
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
            {
                return;
            }

            // Проверка имени метода
            if (memberAccess.Name.Identifier.Text != "UseCors")
            {
                return;
            }

            // Получаем символ метода
            var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;
            if (symbol == null)
            {
                return;
            }

            // Проверка что это именно Microsoft UseCors
            if (symbol.ContainingType.ToDisplayString() !=
                "Microsoft.AspNetCore.Builder.CorsMiddlewareExtensions")
            {
                return;
            }
            
            var assemblyName = context.Compilation.AssemblyName;
            if (assemblyName == "Security.Host")
            {
                return;
            }

            // Создаем диагностику
            var diagnostic = Diagnostic.Create(Rule, memberAccess.Name.GetLocation());

            // Репортим ошибку
            context.ReportDiagnostic(diagnostic);
        }
    }
}