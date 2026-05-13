using Application.Abstractions.Services.Laboratories.Contracts;
using Application.Abstractions.UseCases.User;
using Application.DTO.Laboratories;
using Domain.Laboratories.Enums;

namespace Application.Abstractions.Services.Laboratories;

/// <inheritdoc />
public sealed class LaboratoryService : ILaboratoryService
{
    private const long MaxReportFileSizeBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedReportExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".docx"
    };

    private static readonly HashSet<string> AllowedReportContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private readonly ILaboratoryRepository _repository;
    private readonly ILaboratoryReportFileStorage _fileStorage;
    private readonly ILaboratoryFlagHashService _flagHashService;
    private readonly ICurrentUser _currentUser;

    public LaboratoryService(
        ILaboratoryRepository repository,
        ILaboratoryReportFileStorage fileStorage,
        ILaboratoryFlagHashService flagHashService,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _flagHashService = flagHashService;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public Task<PagedResultDto<GetLaboratoryListItemDto>> GetStudentLaboratoriesAsync(
        GetLaboratoryListRequest request,
        CancellationToken cancellationToken)
    {
        return _repository.GetStudentLaboratoriesAsync(
            _currentUser.UserId,
            NormalizePaging(request),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetLaboratoryDetailsResponse> GetStudentLaboratoryDetailsAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        var laboratory = await _repository.GetStudentLaboratoryDetailsAsync(
            _currentUser.UserId,
            laboratoryId,
            cancellationToken);

        return laboratory ?? throw new LaboratoryException(
            "laboratory.not_found",
            "Лабораторная работа не найдена");
    }

    /// <inheritdoc />
    public async Task<OpenLaboratoryHintResponse> OpenHintAsync(
        Guid laboratoryId,
        Guid hintId,
        CancellationToken cancellationToken)
    {
        var hint = await _repository.OpenHintAsync(
            _currentUser.UserId,
            laboratoryId,
            hintId,
            cancellationToken);

        return hint ?? throw new LaboratoryException(
            "laboratory_hint.not_found",
            "Подсказка не найдена");
    }

    /// <inheritdoc />
    public async Task<SubmitLaboratoryFlagResponse> SubmitFlagAsync(
        Guid laboratoryId,
        SubmitLaboratoryFlagRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Flag))
        {
            throw new LaboratoryException("laboratory_flag.required", "Флаг обязателен");
        }

        if (request.Flag.Length > 500)
        {
            throw new LaboratoryException("laboratory_flag.too_long", "Флаг слишком длинный");
        }

        var details = await _repository.GetTeacherLaboratoryDetailsAsync(laboratoryId, cancellationToken);

        if (details is null || details.DeleteDateUtc.HasValue)
        {
            throw new LaboratoryException("laboratory.not_found", "Лабораторная работа не найдена");
        }

        if (!details.IsPublished)
        {
            throw new LaboratoryException("laboratory.not_published", "Лабораторная работа не опубликована");
        }

        if (!details.HasFlag)
        {
            throw new LaboratoryException("laboratory_flag.disabled", "Для лабораторной работы не включена сдача флага");
        }

        var expectedHash = await GetExpectedFlagHashAsync(laboratoryId, cancellationToken);
        var isCorrect = !string.IsNullOrWhiteSpace(expectedHash)
                        && _flagHashService.VerifyFlag(request.Flag, expectedHash);

        return await _repository.SubmitFlagAttemptAsync(
            _currentUser.UserId,
            laboratoryId,
            _flagHashService.HashFlag(request.Flag),
            _flagHashService.MaskFlag(request.Flag),
            isCorrect,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UploadLaboratoryReportResponse> UploadReportAsync(
        Guid laboratoryId,
        UploadLaboratoryReportFileDto file,
        CancellationToken cancellationToken)
    {
        ValidateReportFile(file);

        var savedFile = await _fileStorage.SaveAsync(file, cancellationToken);

        return await _repository.UploadReportAsync(
            _currentUser.UserId,
            laboratoryId,
            savedFile,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetMyLaboratoryReportResponse> GetMyReportAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        var report = await _repository.GetMyReportAsync(
            _currentUser.UserId,
            laboratoryId,
            cancellationToken);

        return report ?? throw new LaboratoryException(
            "laboratory_report.not_found",
            "Отчет не найден");
    }

    /// <inheritdoc />
    public Task<GetMyProgressResponse> GetMyProgressAsync(CancellationToken cancellationToken)
    {
        return _repository.GetMyProgressAsync(_currentUser.UserId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetMyGradebookResponse> GetMyGradebookAsync(CancellationToken cancellationToken)
    {
        var gradebook = await _repository.GetMyGradebookAsync(_currentUser.UserId, cancellationToken);

        return gradebook ?? throw new LaboratoryException(
            "gradebook.not_found",
            "Запись ведомости не найдена");
    }

    /// <inheritdoc />
    public Task<PagedResultDto<TeacherLaboratoryListItemDto>> GetTeacherLaboratoriesAsync(
        GetTeacherLaboratoryListRequest request,
        CancellationToken cancellationToken)
    {
        return _repository.GetTeacherLaboratoriesAsync(NormalizePaging(request), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetTeacherLaboratoryDetailsResponse> GetTeacherLaboratoryDetailsAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        var laboratory = await _repository.GetTeacherLaboratoryDetailsAsync(
            laboratoryId,
            cancellationToken);

        return laboratory ?? throw new LaboratoryException(
            "laboratory.not_found",
            "Лабораторная работа не найдена");
    }

    /// <inheritdoc />
    public Task<Guid> CreateLaboratoryAsync(
        CreateLaboratoryRequest request,
        CancellationToken cancellationToken)
    {
        ValidateLaboratory(request, expectedFlagRequired: request.HasFlag);
        var expectedFlagHash = request.HasFlag ? _flagHashService.HashFlag(request.ExpectedFlag!) : null;

        return _repository.CreateLaboratoryAsync(request, expectedFlagHash, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateLaboratoryAsync(
        Guid laboratoryId,
        UpdateLaboratoryRequest request,
        CancellationToken cancellationToken)
    {
        ValidateLaboratory(request, expectedFlagRequired: false);
        var updateFlagHash = request.HasFlag && !string.IsNullOrWhiteSpace(request.ExpectedFlag);
        var expectedFlagHash = updateFlagHash ? _flagHashService.HashFlag(request.ExpectedFlag!) : null;

        return _repository.UpdateLaboratoryAsync(
            laboratoryId,
            request,
            expectedFlagHash,
            updateFlagHash,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteLaboratoryAsync(Guid laboratoryId, CancellationToken cancellationToken)
    {
        return _repository.DeleteLaboratoryAsync(laboratoryId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedResultDto<TeacherReportListItemDto>> GetTeacherReportsAsync(
        GetTeacherReportListRequest request,
        CancellationToken cancellationToken)
    {
        return _repository.GetTeacherReportsAsync(NormalizePaging(request), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetTeacherReportDetailsResponse> GetTeacherReportDetailsAsync(
        Guid reportId,
        CancellationToken cancellationToken)
    {
        var report = await _repository.GetTeacherReportDetailsAsync(reportId, cancellationToken);

        return report ?? throw new LaboratoryException(
            "laboratory_report.not_found",
            "Отчет не найден");
    }

    /// <inheritdoc />
    public async Task<LaboratoryReportFileDto> GetReportFileAsync(
        Guid reportId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var file = await _repository.GetReportFileAsync(reportId, versionId, cancellationToken);

        return file ?? throw new LaboratoryException(
            "laboratory_report_version.not_found",
            "Версия отчета не найдена");
    }

    /// <inheritdoc />
    public Task<ReviewLaboratoryReportResponse> ReviewReportAsync(
        Guid reportId,
        ReviewLaboratoryReportRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Status is not LaboratoryReportStatus.Accepted and not LaboratoryReportStatus.Rejected)
        {
            throw new LaboratoryException("laboratory_review.invalid_status", "Некорректный статус проверки");
        }

        if (request.Status == LaboratoryReportStatus.Accepted && !request.Points.HasValue)
        {
            throw new LaboratoryException("laboratory_review.points_required", "Баллы обязательны при принятии отчета");
        }

        if (request.Points is < 0)
        {
            throw new LaboratoryException("laboratory_review.points_out_of_range", "Баллы не могут быть отрицательными");
        }

        if (request.Status == LaboratoryReportStatus.Rejected && string.IsNullOrWhiteSpace(request.Comment))
        {
            throw new LaboratoryException("laboratory_review.comment_required", "Комментарий обязателен при отклонении отчета");
        }

        if (request.Comment?.Length > 4000)
        {
            throw new LaboratoryException("laboratory_review.comment_too_long", "Комментарий слишком длинный");
        }

        return _repository.ReviewReportAsync(
            _currentUser.UserId,
            reportId,
            request,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedResultDto<TeacherGradebookItemDto>> GetTeacherGradebookAsync(
        GetTeacherGradebookRequest request,
        CancellationToken cancellationToken)
    {
        return _repository.GetTeacherGradebookAsync(NormalizePaging(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task<TeacherGradebookItemDto> UpdateGradebookAsync(
        Guid studentId,
        UpdateTeacherGradebookRequest request,
        CancellationToken cancellationToken)
    {
        if (request.AttendancePercent is < 0 or > 100)
        {
            throw new LaboratoryException("gradebook.attendance_out_of_range", "Посещаемость должна быть от 0 до 100");
        }

        return _repository.UpdateGradebookAsync(
            _currentUser.UserId,
            studentId,
            request,
            cancellationToken);
    }

    private async Task<string?> GetExpectedFlagHashAsync(Guid laboratoryId, CancellationToken cancellationToken)
    {
        var laboratory = await _repository.GetTeacherLaboratoryDetailsAsync(laboratoryId, cancellationToken);

        return laboratory?.HasExpectedFlag == true
            ? await _repository.GetExpectedFlagHashAsync(laboratoryId, cancellationToken)
            : null;
    }

    private static void ValidateReportFile(UploadLaboratoryReportFileDto file)
    {
        if (file.Length <= 0)
        {
            throw new LaboratoryException("laboratory_report.file_required", "Файл отчета обязателен");
        }

        if (file.Length > MaxReportFileSizeBytes)
        {
            throw new LaboratoryException("laboratory_report.file_too_large", "Размер файла отчета не должен превышать 10 MB");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedReportExtensions.Contains(extension) || !AllowedReportContentTypes.Contains(file.ContentType))
        {
            throw new LaboratoryException("laboratory_report.invalid_file_type", "Разрешены только PDF и DOCX файлы");
        }
    }

    private static void ValidateLaboratory(CreateLaboratoryRequest request, bool expectedFlagRequired)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new LaboratoryException("laboratory.title_required", "Название лабораторной работы обязательно");
        }

        if (request.Title.Length > 255)
        {
            throw new LaboratoryException("laboratory.title_too_long", "Название лабораторной работы слишком длинное");
        }

        if (request.ShortDescription.Length > 1000)
        {
            throw new LaboratoryException("laboratory.short_description_too_long", "Краткое описание слишком длинное");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new LaboratoryException("laboratory.description_required", "Описание лабораторной работы обязательно");
        }

        if (!Enum.IsDefined(request.Difficulty))
        {
            throw new LaboratoryException("laboratory.difficulty_invalid", "Некорректная сложность лабораторной работы");
        }

        if (string.IsNullOrWhiteSpace(request.Block))
        {
            throw new LaboratoryException("laboratory.block_required", "Блок лабораторной работы обязателен");
        }

        if (request.MaxPoints <= 0)
        {
            throw new LaboratoryException("laboratory.max_points_invalid", "Максимальный балл должен быть больше 0");
        }

        if (!string.IsNullOrWhiteSpace(request.EnvironmentUrl)
            && !Uri.TryCreate(request.EnvironmentUrl, UriKind.Absolute, out _))
        {
            throw new LaboratoryException("laboratory.environment_url_invalid", "Некорректная ссылка на окружение");
        }

        if (expectedFlagRequired && string.IsNullOrWhiteSpace(request.ExpectedFlag))
        {
            throw new LaboratoryException("laboratory.expected_flag_required", "Флаг обязателен для лабораторной работы со сдачей флага");
        }

        var duplicateHintOrderNumber = request.Hints
            .GroupBy(x => x.OrderNumber)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicateHintOrderNumber is not null)
        {
            throw new LaboratoryException("laboratory_hint.order_duplicate", "Номера подсказок должны быть уникальны");
        }

        foreach (var hint in request.Hints)
        {
            if (hint.OrderNumber <= 0)
            {
                throw new LaboratoryException("laboratory_hint.order_invalid", "Номер подсказки должен быть больше 0");
            }

            if (string.IsNullOrWhiteSpace(hint.Text))
            {
                throw new LaboratoryException("laboratory_hint.text_required", "Текст подсказки обязателен");
            }

            if (hint.PenaltyPoints < 0)
            {
                throw new LaboratoryException("laboratory_hint.penalty_invalid", "Штраф за подсказку не может быть отрицательным");
            }
        }
    }

    private static GetLaboratoryListRequest NormalizePaging(GetLaboratoryListRequest request)
    {
        return request with
        {
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, 100)
        };
    }

    private static GetTeacherLaboratoryListRequest NormalizePaging(GetTeacherLaboratoryListRequest request)
    {
        return request with
        {
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, 100)
        };
    }

    private static GetTeacherReportListRequest NormalizePaging(GetTeacherReportListRequest request)
    {
        return request with
        {
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, 100)
        };
    }

    private static GetTeacherGradebookRequest NormalizePaging(GetTeacherGradebookRequest request)
    {
        return request with
        {
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, 100)
        };
    }
}
