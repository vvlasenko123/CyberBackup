using Infrastructure.Exceptions;
using Infrastructure.Exceptions.Base;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Errors;

internal static class ApiErrors
{
    internal static Task HandleExceptionAsync(HttpContext context)
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()!.Error;
        var (status, error) = FromException(exception);
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(error, context.RequestAborted);
    }

    internal static (int Status, ApiError Error) FromException(Exception exception) => exception switch
    {
        BaseException error => (StatusFor(error.Code), new ApiError(error.Code, error.Message)),
        BadHttpRequestException error => (error.StatusCode, FromStatus(error.StatusCode)),
        _ => (StatusCodes.Status500InternalServerError, FromStatus(StatusCodes.Status500InternalServerError))
    };

    private static int StatusFor(string code) => code switch
    {
        ErrorCodes.AuthAccountDeactivated or ErrorCodes.AuthInvalidCredentials or ErrorCodes.AuthInvalidToken
            => StatusCodes.Status401Unauthorized,
        ErrorCodes.LaboratoryGroupsForbidden or ErrorCodes.UserRoleForbidden or ErrorCodes.UserSelfDeleteForbidden
            => StatusCodes.Status403Forbidden,
        ErrorCodes.GroupNotFound or ErrorCodes.GroupMemberNotFound or ErrorCodes.UserNotFound or
        ErrorCodes.GradebookNotFound or ErrorCodes.LaboratoryNotFound or ErrorCodes.LaboratoryHintNotFound or
        ErrorCodes.LaboratoryReportNotFound or ErrorCodes.LaboratoryReportVersionNotFound or
        ErrorCodes.QuestionNotFound or ErrorCodes.QuestionAttachmentNotFound
            => StatusCodes.Status404NotFound,
        ErrorCodes.UserEmailTaken or ErrorCodes.QuestionClosed or ErrorCodes.LaboratoryFlagDisabled or
        ErrorCodes.LaboratoryReportDeadlineExpired or ErrorCodes.LaboratoryReportPendingReview or
        ErrorCodes.LaboratoryReportAcceptedFinal or ErrorCodes.LaboratoryReportNotPendingReview or
        ErrorCodes.LaboratoryReportResubmitForbidden
            => StatusCodes.Status409Conflict,
        ErrorCodes.LaboratoryReportFileTooLarge => StatusCodes.Status413PayloadTooLarge,
        _ => StatusCodes.Status400BadRequest
    };

    internal static ApiError FromStatus(int status) => status switch
    {
        StatusCodes.Status400BadRequest => new(ErrorCodes.RequestInvalid, "Проверьте данные запроса"),
        StatusCodes.Status401Unauthorized => new(ErrorCodes.AuthRequired, "Требуется авторизация"),
        StatusCodes.Status403Forbidden => new(ErrorCodes.AccessForbidden, "Недостаточно прав для выполнения операции"),
        StatusCodes.Status404NotFound => new(ErrorCodes.ResourceNotFound, "Ресурс не найден"),
        StatusCodes.Status405MethodNotAllowed => new(ErrorCodes.RequestMethodNotAllowed, "Метод запроса не поддерживается"),
        StatusCodes.Status409Conflict => new(ErrorCodes.ResourceConflict, "Операция конфликтует с текущим состоянием ресурса"),
        StatusCodes.Status413PayloadTooLarge => new(ErrorCodes.RequestTooLarge, "Превышен допустимый размер запроса"),
        StatusCodes.Status415UnsupportedMediaType => new(ErrorCodes.RequestUnsupportedMediaType, "Формат запроса не поддерживается"),
        >= 500 => new(ErrorCodes.ServerInternalError, "Внутренняя ошибка сервера"),
        _ => new(ErrorCodes.RequestRejected, "Не удалось обработать запрос")
    };
}
