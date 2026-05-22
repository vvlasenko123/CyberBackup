using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р—Р°РїСЂРѕСЃ РѕР±РЅРѕРІР»РµРЅРёСЏ РІРµРґРѕРјРѕСЃС‚Рё
/// </summary>
public sealed record UpdateTeacherGradebookRequest
{
    public decimal AttendancePercent { get; init; }
    public bool IsExamAllowed { get; init; }
    public bool HasAutomaticGrade { get; init; }
}

