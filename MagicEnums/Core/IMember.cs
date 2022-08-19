namespace CodeChops.MagicEnums.Core;

public interface IMember<out TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue>
{
	/// <summary>
	/// The value of the enum member.
	/// </summary>
	TValue Value { get; }
}

public interface IMember
{
	/// <summary>
	/// The value of the enum member.
	/// </summary>
	string Name { get; }
}