namespace Infrastructure.Core.DDD.ValueObject.Contracts;

/// <summary>
/// Value Object
/// </summary>
internal interface IValueObject<TType> : IEquatable<TType>
{
}