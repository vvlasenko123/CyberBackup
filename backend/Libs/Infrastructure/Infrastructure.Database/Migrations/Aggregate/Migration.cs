using Infrastructure.Core.DDD.Aggregate;

namespace Infrastructure.Database.Migrations.Aggregate;

/// <summary>
/// Сущность примененной миграции базы данных
/// </summary>
public sealed class Migration : AggregateRoot<string>
{
    /// <summary>
    /// Дата применения миграции
    /// </summary>
    public DateTime AppliedAt { get; private set; }

    public Migration(string id, DateTime appliedAt) 
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Идентификатор миграции не должен быть пустым", nameof(id));
        }

        AppliedAt = appliedAt;
    }
}