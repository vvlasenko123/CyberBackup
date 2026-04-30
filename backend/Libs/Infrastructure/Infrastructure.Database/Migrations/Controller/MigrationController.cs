using Infrastructure.Core.Controllers.Internal;
using Infrastructure.Database.Migrations.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Database.Migrations.Controller;

/// <summary>
/// Контроллер для миграций
/// </summary>
[ApiController]
[Route("migrations")]
public class MigrationController : InternalController
{
    private readonly IMigrationRepository _migrationRepository;

    public MigrationController(IMigrationRepository migrationRepository)
    {
        _migrationRepository = migrationRepository;
    }

    /// <summary>
    /// Получение списка примененных миграций
    /// </summary>
    [HttpGet("get-all")]
    public async Task<IActionResult> GetMigrations(CancellationToken cancellationToken)
    {
        var result = await _migrationRepository.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Получение последней примененной миграции
    /// </summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestMigration(CancellationToken cancellationToken)
    {
        var result = await _migrationRepository.GetLatestAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Применение всех непримененных миграций
    /// </summary>
    [HttpPost("apply")]
    public async Task<IActionResult> UpMigration(CancellationToken cancellationToken)
    {
        await _migrationRepository.MigrateUpAsync(cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Откат последней примененной миграции
    /// </summary>
    [HttpPost("rollback")]
    public async Task<IActionResult> DownMigration(CancellationToken cancellationToken)
    {
        await _migrationRepository.MigrateDownAsync(cancellationToken);
        return Ok();
    }
}