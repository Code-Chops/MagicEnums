using System.Runtime.CompilerServices;
using CodeChops.GenericMath;
using CodeChops.MagicEnums.Core;

namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with an integer value.
/// </summary>
/// <typeparam name="TEnum">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicEnum<TEnum> : MagicEnum<TEnum, int>
	where TEnum : MagicEnum<TEnum>
{
}

/// <summary>
/// An enum with an integral value.
/// </summary>
/// <typeparam name="TEnum">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TValue">The integral type.</typeparam>
public abstract record MagicEnum<TEnum, TValue> : MagicEnumCore<TEnum, TValue>
	where TEnum : MagicEnum<TEnum, TValue>
	where TValue : struct, IComparable<TValue>, IEquatable<TValue>, IConvertible
{
	#region Comparison
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual int CompareTo(MagicEnum<TEnum, TValue> other) => Value.CompareTo(other.Value);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <(MagicEnum<TEnum, TValue> left, MagicEnum<TEnum, TValue> right)	=> left.CompareTo(right) <	0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <=(MagicEnum<TEnum, TValue> left, MagicEnum<TEnum, TValue> right)	=> left.CompareTo(right) <= 0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >(MagicEnum<TEnum, TValue> left, MagicEnum<TEnum, TValue> right)	=> left.CompareTo(right) >	0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >=(MagicEnum<TEnum, TValue> left, MagicEnum<TEnum, TValue> right)	=> left.CompareTo(right) >= 0;
	#endregion

	/// <summary>
	/// Used for auto-incrementing when no member value has been provided.
	/// </summary>
	private static Number<TValue>? LastInsertedNumber { get; set; }
	/// <summary>
	/// Locks the last inserted number which is being used for auto-incrementing the enum value.
	/// </summary>
	private static readonly object LockLastInsertedNumber = new();

	/// <summary>
	/// Creates a new enum member with an incremental value.
	/// </summary>
	/// <param name="enforcedName">
	/// The name of the new member.
	/// Don't provide this parameter, so the property name of the enum will automaticaly be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten. 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	public static TEnum CreateMember([CallerMemberName] string? enforcedName = null)
	{
		if (IsInConcurrentState)
			lock (LockLastInsertedNumber) IncrementLastInsertedNumber();
		else
			IncrementLastInsertedNumber();

		var id = MagicEnumCore<TEnum, TValue>.CreateMember(LastInsertedNumber ?? GetLastInsertedValue(), enforcedName!);
		return id;


		static void IncrementLastInsertedNumber()
		{
			if (LastInsertedNumber is null)
				LastInsertedNumber = new();
			else
				LastInsertedNumber!++;
		}
	}

	/// <summary>
	/// Creates a new enum member with the provided integral value.
	/// </summary>
	/// <param name="enforcedName">
	/// The name of the new member.
	/// Don't provide this parameter, so the property name of the enum will automaticaly be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten. 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected static new TEnum CreateMember(TValue value, [CallerMemberName] string? enforcedName = null)
	{
		if (IsInConcurrentState)
			lock (LockLastInsertedNumber) LastInsertedNumber = value;
		else
			LastInsertedNumber = value;

		return MagicEnumCore<TEnum, TValue>.CreateMember(value, enforcedName);
	}
}