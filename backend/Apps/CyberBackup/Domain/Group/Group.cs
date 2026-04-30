using Domain.Shared;
using Infrastructure.Core.DDD.Aggregate;

namespace Domain.Group;

/// <summary>
/// Группа
/// </summary>
public sealed class Group : AggregateRoot<Guid>
{
    public Group(GroupId groupId) : base(groupId.Value)
    {
    }
}