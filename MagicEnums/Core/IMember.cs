namespace CodeChops.MagicEnums.Core;

public interface IMember<out TValue> : IMember, IId<TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue>, IConvertible
{
}

public interface IMember
{
	/// <summary>
	/// The value of the enum member.
	/// </summary>
	string Name { get; }
}