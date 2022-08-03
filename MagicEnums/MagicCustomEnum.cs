namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with a custom type as member value.
/// Use <see cref="CodeChops.MagicEnums.Core.MagicEnumCore{TSelf, TValue}.CreateMember(TValue, string)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TValue">The type of the value of the enum.</typeparam>
public abstract record MagicCustomEnum<TSelf, TValue> : MagicEnumCore<TSelf, TValue>
	where TSelf : MagicCustomEnum<TSelf, TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue>;