using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р’РµРґРѕРјРѕСЃС‚СЊ С‚РµРєСѓС‰РµРіРѕ СЃС‚СѓРґРµРЅС‚Р°
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

