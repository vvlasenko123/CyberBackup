using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Страничный результат
/// </summary>
public sealed record PagedResultDto<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int Page,
    int PageSize);

/// <summary>
/// Запрос списка лабораторных работ для студента
/// </summary>
public sealed record GetLaboratoryListRequest
{
    public string? Block { get; init; }
    public LaboratoryDifficulty? Difficulty { get; init; }
    public StudentLaboratoryStatus? Status { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Элемент списка лабораторных работ для студента
/// </summary>
public sealed record GetLaboratoryListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public LaboratoryDifficulty Difficulty { get; init; }
    public string DifficultyName { get; init; } = string.Empty;
    public string Block { get; init; } = string.Empty;
    public int MaxPoints { get; init; }
    public int EarnedPoints { get; init; }
    public StudentLaboratoryStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public bool IsCompleted { get; init; }
    public int ProgressPercent { get; init; }
    public int SortOrder { get; init; }
}

/// <summary>
/// Детали лабораторной работы для студента
/// </summary>
public sealed record GetLaboratoryDetailsResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Narrative { get; init; } = string.Empty;
    public string Goal { get; init; } = string.Empty;
    public string? EnvironmentUrl { get; init; }
    public string? Credentials { get; init; }
    public LaboratoryDifficulty Difficulty { get; init; }
    public string DifficultyName { get; init; } = string.Empty;
    public string Block { get; init; } = string.Empty;
    public int MaxPoints { get; init; }
    public int EarnedPoints { get; init; }
    public bool HasFlag { get; init; }
    public bool FlagAlreadySubmitted { get; init; }
    public LaboratoryReportStatus ReportStatus { get; init; }
    public bool AllowReportUpload { get; init; }
    public bool CanResubmitReport { get; init; }
    public IReadOnlyCollection<LaboratoryHintDto> Hints { get; init; } = [];
    public GetMyLaboratoryReportResponse? Report { get; init; }
}

/// <summary>
/// Подсказка лабораторной работы
/// </summary>
public sealed record LaboratoryHintDto
{
    public Guid Id { get; init; }
    public int OrderNumber { get; init; }
    public string? Title { get; init; }
    public int PenaltyPoints { get; init; }
    public bool IsOpened { get; init; }
    public string? Text { get; init; }
}

/// <summary>
/// Ответ открытия подсказки
/// </summary>
public sealed record OpenLaboratoryHintResponse
{
    public Guid HintId { get; init; }
    public int OrderNumber { get; init; }
    public string Text { get; init; } = string.Empty;
    public int PenaltyPoints { get; init; }
    public int TotalPenaltyPoints { get; init; }
    public int AvailablePoints { get; init; }
}

/// <summary>
/// Запрос сдачи флага
/// </summary>
public sealed record SubmitLaboratoryFlagRequest
{
    public string Flag { get; init; } = string.Empty;
}

/// <summary>
/// Ответ сдачи флага
/// </summary>
public sealed record SubmitLaboratoryFlagResponse
{
    public bool IsCorrect { get; init; }
    public string Message { get; init; } = string.Empty;
    public int EarnedPoints { get; init; }
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// Сохраненный файл отчета
/// </summary>
public sealed record SavedLaboratoryReportFileDto(
    string StoragePath,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes);

/// <summary>
/// Ответ загрузки отчета
/// </summary>
public sealed record UploadLaboratoryReportResponse
{
    public Guid ReportId { get; init; }
    public Guid VersionId { get; init; }
    public int VersionNumber { get; init; }
    public LaboratoryReportStatus Status { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public DateTimeOffset CreateDateUtc { get; init; }
}

/// <summary>
/// Отчет текущего студента
/// </summary>
public sealed record GetMyLaboratoryReportResponse
{
    public Guid ReportId { get; init; }
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public string? TeacherComment { get; init; }
    public bool AllowResubmit { get; init; }
    public IReadOnlyCollection<LaboratoryReportVersionDto> Versions { get; init; } = [];
}

/// <summary>
/// Версия отчета
/// </summary>
public sealed record LaboratoryReportVersionDto
{
    public Guid VersionId { get; init; }
    public int VersionNumber { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string? ContentType { get; init; }
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public string? TeacherComment { get; init; }
    public DateTimeOffset CreateDateUtc { get; init; }
    public Guid? CheckedByTeacherId { get; init; }
    public string? CheckedByTeacherFullName { get; init; }
    public DateTimeOffset? CheckedDateUtc { get; init; }
    public string? FileDownloadUrl { get; init; }
}

/// <summary>
/// Прогресс текущего студента
/// </summary>
public sealed record GetMyProgressResponse
{
    public int TotalLaboratories { get; init; }
    public int CompletedLaboratories { get; init; }
    public int PendingReviewLaboratories { get; init; }
    public int RejectedLaboratories { get; init; }
    public int TotalPoints { get; init; }
    public int EarnedPoints { get; init; }
    public int ProgressPercent { get; init; }
    public IReadOnlyCollection<MyProgressLaboratoryDto> Laboratories { get; init; } = [];
}

/// <summary>
/// Прогресс по лабораторной работе
/// </summary>
public sealed record MyProgressLaboratoryDto
{
    public Guid LaboratoryId { get; init; }
    public string Title { get; init; } = string.Empty;
    public StudentLaboratoryStatus Status { get; init; }
    public int EarnedPoints { get; init; }
    public int MaxPoints { get; init; }
}

/// <summary>
/// Ведомость текущего студента
/// </summary>
public sealed record GetMyGradebookResponse
{
    public GradebookStudentDto Student { get; init; } = new();
    public decimal AttendancePercent { get; init; }
    public bool IsExamAllowed { get; init; }
    public bool HasAutomaticGrade { get; init; }
    public int TotalPoints { get; init; }
    public IReadOnlyCollection<MyGradebookLaboratoryDto> Laboratories { get; init; } = [];
}

/// <summary>
/// Студент в ведомости
/// </summary>
public sealed record GradebookStudentDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? GroupName { get; init; }
}

/// <summary>
/// Лабораторная работа в ведомости
/// </summary>
public sealed record MyGradebookLaboratoryDto
{
    public Guid LaboratoryId { get; init; }
    public string Title { get; init; } = string.Empty;
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public int MaxPoints { get; init; }
    public string? TeacherComment { get; init; }
}

/// <summary>
/// Запрос списка лабораторных для преподавателя
/// </summary>
public sealed record GetTeacherLaboratoryListRequest
{
    public string? Block { get; init; }
    public LaboratoryDifficulty? Difficulty { get; init; }
    public bool? IsPublished { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Элемент списка лабораторных для преподавателя
/// </summary>
public sealed record TeacherLaboratoryListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public LaboratoryDifficulty Difficulty { get; init; }
    public string Block { get; init; } = string.Empty;
    public int MaxPoints { get; init; }
    public bool HasFlag { get; init; }
    public bool IsPublished { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreateDateUtc { get; init; }
    public DateTimeOffset? UpdateDateUtc { get; init; }
}

/// <summary>
/// Детали лабораторной для преподавателя
/// </summary>
public sealed record GetTeacherLaboratoryDetailsResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Narrative { get; init; } = string.Empty;
    public string Goal { get; init; } = string.Empty;
    public string? EnvironmentUrl { get; init; }
    public string? Credentials { get; init; }
    public LaboratoryDifficulty Difficulty { get; init; }
    public string Block { get; init; } = string.Empty;
    public int MaxPoints { get; init; }
    public bool HasFlag { get; init; }
    public bool HasExpectedFlag { get; init; }
    public bool IsPublished { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreateDateUtc { get; init; }
    public DateTimeOffset? UpdateDateUtc { get; init; }
    public DateTimeOffset? DeleteDateUtc { get; init; }
    public IReadOnlyCollection<LaboratoryHintInputDto> Hints { get; init; } = [];
}

/// <summary>
/// Запрос создания лабораторной
/// </summary>
public record CreateLaboratoryRequest
{
    public string Title { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Narrative { get; init; } = string.Empty;
    public string Goal { get; init; } = string.Empty;
    public string? EnvironmentUrl { get; init; }
    public string? Credentials { get; init; }
    public LaboratoryDifficulty Difficulty { get; init; }
    public string Block { get; init; } = string.Empty;
    public int MaxPoints { get; init; }
    public bool HasFlag { get; init; }
    public string? ExpectedFlag { get; init; }
    public bool IsPublished { get; init; }
    public int SortOrder { get; init; }
    public IReadOnlyCollection<LaboratoryHintInputDto> Hints { get; init; } = [];
}

/// <summary>
/// Запрос обновления лабораторной
/// </summary>
public sealed record UpdateLaboratoryRequest : CreateLaboratoryRequest
{
}

/// <summary>
/// Подсказка в запросе преподавателя
/// </summary>
public sealed record LaboratoryHintInputDto
{
    public Guid? Id { get; init; }
    public int OrderNumber { get; init; }
    public string? Title { get; init; }
    public string Text { get; init; } = string.Empty;
    public int PenaltyPoints { get; init; }
}

/// <summary>
/// Запрос списка отчетов для преподавателя
/// </summary>
public sealed record GetTeacherReportListRequest
{
    public LaboratoryReportStatus? Status { get; init; }
    public Guid? LaboratoryId { get; init; }
    public string? Search { get; init; }
    public string? GroupName { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Элемент списка отчетов для преподавателя
/// </summary>
public sealed record TeacherReportListItemDto
{
    public Guid ReportId { get; init; }
    public Guid LaboratoryId { get; init; }
    public string LaboratoryTitle { get; init; } = string.Empty;
    public Guid StudentId { get; init; }
    public string StudentFullName { get; init; } = string.Empty;
    public string? GroupName { get; init; }
    public int CurrentVersionNumber { get; init; }
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public int MaxPoints { get; init; }
    public bool AllowResubmit { get; init; }
    public DateTimeOffset CreateDateUtc { get; init; }
    public DateTimeOffset? UpdateDateUtc { get; init; }
    public DateTimeOffset LastSubmitDateUtc { get; init; }
}

/// <summary>
/// Детали отчета для преподавателя
/// </summary>
public sealed record GetTeacherReportDetailsResponse
{
    public Guid ReportId { get; init; }
    public TeacherReportLaboratoryDto Laboratory { get; init; } = new();
    public GradebookStudentDto Student { get; init; } = new();
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public string? TeacherComment { get; init; }
    public bool AllowResubmit { get; init; }
    public IReadOnlyCollection<LaboratoryReportVersionDto> Versions { get; init; } = [];
}

/// <summary>
/// Лабораторная работа в отчете преподавателя
/// </summary>
public sealed record TeacherReportLaboratoryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int MaxPoints { get; init; }
}

/// <summary>
/// Запрос проверки отчета
/// </summary>
public sealed record ReviewLaboratoryReportRequest
{
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public string? Comment { get; init; }
    public bool AllowResubmit { get; init; }
}

/// <summary>
/// Ответ проверки отчета
/// </summary>
public sealed record ReviewLaboratoryReportResponse
{
    public Guid ReportId { get; init; }
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public string? TeacherComment { get; init; }
    public bool AllowResubmit { get; init; }
    public DateTimeOffset CheckedDateUtc { get; init; }
}

/// <summary>
/// Файл версии отчета
/// </summary>
public sealed record LaboratoryReportFileDto
{
    public string StoragePath { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

/// <summary>
/// Запрос ведомости для преподавателя
/// </summary>
public sealed record GetTeacherGradebookRequest
{
    public string? GroupName { get; init; }
    public bool? IsExamAllowed { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Элемент ведомости преподавателя
/// </summary>
public sealed record TeacherGradebookItemDto
{
    public Guid StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? GroupName { get; init; }
    public decimal AttendancePercent { get; init; }
    public bool IsExamAllowed { get; init; }
    public bool HasAutomaticGrade { get; init; }
    public int TotalPoints { get; init; }
    public int CompletedLaboratories { get; init; }
    public int TotalLaboratories { get; init; }
}

/// <summary>
/// Запрос обновления ведомости
/// </summary>
public sealed record UpdateTeacherGradebookRequest
{
    public decimal AttendancePercent { get; init; }
    public bool IsExamAllowed { get; init; }
    public bool HasAutomaticGrade { get; init; }
}
