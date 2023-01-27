namespace CodeChops.MagicEnums.Core;

/// <inheritdoc cref="IMember"/>
public interface IMember<out TValue> : IMember
	where TValue : IEquatable<TValue>, IComparable<TValue>
{
	TValue Value { get; }
}

/// <summary>
/// A magic enum member.
/// </summary>
public interface IMember
{
	/// <summary>
	/// The value of the enum member.
	/// </summary>
	string Name { get; }
}