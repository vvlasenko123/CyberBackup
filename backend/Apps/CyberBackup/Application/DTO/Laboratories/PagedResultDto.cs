using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РЎС‚СЂР°РЅРёС‡РЅС‹Р№ СЂРµР·СѓР»СЊС‚Р°С‚
/// </summary>
public sealed record PagedResultDto<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int Page,
    int PageSize);

