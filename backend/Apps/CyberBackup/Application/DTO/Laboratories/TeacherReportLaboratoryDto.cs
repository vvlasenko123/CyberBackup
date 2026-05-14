using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р›Р°Р±РѕСЂР°С‚РѕСЂРЅР°СЏ СЂР°Р±РѕС‚Р° РІ РѕС‚С‡РµС‚Рµ РїСЂРµРїРѕРґР°РІР°С‚РµР»СЏ
/// </summary>
public sealed record TeacherReportLaboratoryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int MaxPoints { get; init; }
}

