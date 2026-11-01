using Application.Abstractions.Services;
using Application.Abstractions.Services.Questions;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Errors;

/// <summary>
/// Глобальный обработчик необработанных исключений: отвечает JSON-телом с полем message
/// (и полем code для ошибок с машинным кодом)
/// </summary>
public static class ApiExceptionHandler
{
    /// <summary>
    /// Записать ответ об ошибке для исключения из <see cref="IExceptionHandlerFeature"/>
    /// </summary>
    public static async Task HandleAsync(HttpContext ctx)
    {
        var feature = ctx.Features.Get<IExceptionHandlerFeature>();
        var ex = feature?.Error;

        ctx.Response.ContentType = "application/json; charset=utf-8";

        ctx.Response.StatusCode = ex switch
        {
            CodedBadRequestException => 400,
            // Например, 413 при превышении лимита тела запроса на уровне Kestrel
            BadHttpRequestException { StatusCode: >= 400 and < 500 } badRequest => badRequest.StatusCode,
            QuestionException { Code: "question.not_found" or "question.attachment_not_found" } => 404,
            QuestionException => 400,
            InvalidOperationException => 400,
            UnauthorizedAccessException => 403,
            KeyNotFoundException => 404,
            _ => 500
        };

        var message = ctx.Response.StatusCode switch
        {
            500 => "Внутренняя ошибка сервера",
            413 => "Размер запроса превышает допустимый",
            _ => ex?.Message ?? "Ошибка запроса"
        };

        if (ex is CodedBadRequestException coded)
        {
            await ctx.Response.WriteAsJsonAsync(new { code = coded.Code, message });

            return;
        }

        await ctx.Response.WriteAsJsonAsync(new { message });
    }
}
