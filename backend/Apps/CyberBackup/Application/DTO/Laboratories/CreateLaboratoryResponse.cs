using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РћС‚РІРµС‚ СЃРѕР·РґР°РЅРёСЏ Р»Р°Р±РѕСЂР°С‚РѕСЂРЅРѕР№ СЂР°Р±РѕС‚С‹
/// </summary>
public sealed record CreateLaboratoryResponse
{
    public Guid Id { get; init; }
}

