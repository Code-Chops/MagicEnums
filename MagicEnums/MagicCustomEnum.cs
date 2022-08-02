namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with a custom type as member value.
/// Use <see cref="CodeChops.MagicEnums.Core.MagicEnumCore{TSelf, TValue}.CreateMember(TValue, string)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TValue">The type of the value of the enum.</typeparam>
public abstract record MagicCustomEnum<TSelf, TValue> : MagicEnumCore<TSelf, TValue>, IComparable<MagicCustomEnum<TSelf, TValue>>
	where TSelf : MagicCustomEnum<TSelf, TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue>
{
	#region Comparison

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(MagicCustomEnum<TSelf, TValue>? other)
	{
		if (other is null) throw new ArgumentNullException(nameof(other));
		return this.Value.CompareTo(other.Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <(MagicCustomEnum<TSelf, TValue> left, MagicCustomEnum<TSelf, TValue> right)	=> left.CompareTo(right) <	0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <=(MagicCustomEnum<TSelf, TValue> left, MagicCustomEnum<TSelf, TValue> right)	=> left.CompareTo(right) <= 0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >(MagicCustomEnum<TSelf, TValue> left, MagicCustomEnum<TSelf, TValue> right)	=> left.CompareTo(right) >	0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >=(MagicCustomEnum<TSelf, TValue> left, MagicCustomEnum<TSelf, TValue> right)	=> left.CompareTo(right) >= 0;
	
	#endregion
}