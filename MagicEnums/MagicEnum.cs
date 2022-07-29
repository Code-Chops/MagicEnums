using System.Runtime.CompilerServices;
using CodeChops.GenericMath;
using CodeChops.MagicEnums.Core;

namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with an integer value. 
/// Use <see cref="MagicEnum{TSelf, TValue}.CreateMember(string?)"/> 
/// or <see cref="CodeChops.MagicEnums.MagicEnum{TSelf, TValue}.CreateMember(TValue, string?)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicEnum<TSelf> : MagicEnum<TSelf, int>
	where TSelf : MagicEnum<TSelf>;

/// <summary>
/// An enum with an integral value.
/// Use <see cref="MagicEnum{TSelf, TValue}.CreateMember(string?)"/> 
/// or <see cref="CodeChops.MagicEnums.MagicEnum{TSelf, TValue}.CreateMember(TValue, string?)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TValue">The integral type.</typeparam>
public abstract record MagicEnum<TSelf, TValue> : MagicEnumCore<TSelf, TValue>
	where TSelf : MagicEnum<TSelf, TValue>
	where TValue : struct, IComparable<TValue>, IEquatable<TValue>, IConvertible
{
	#region Comparison
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int CompareTo(TValue other) => this.Value.CompareTo(other);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <(MagicEnum<TSelf, TValue> left, MagicEnum<TSelf, TValue> right)	=> left.CompareTo(right) <	0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <=(MagicEnum<TSelf, TValue> left, MagicEnum<TSelf, TValue> right)	=> left.CompareTo(right) <= 0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >(MagicEnum<TSelf, TValue> left, MagicEnum<TSelf, TValue> right)	=> left.CompareTo(right) >	0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >=(MagicEnum<TSelf, TValue> left, MagicEnum<TSelf, TValue> right)	=> left.CompareTo(right) >= 0;
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
	/// Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten. 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	public static TSelf CreateMember([CallerMemberName] string? enforcedName = null)
	{
		Number<TValue>? lastInsertedNumber;
		
		if (IsInConcurrentState)
			lock (LockLastInsertedNumber) lastInsertedNumber = IncrementLastInsertedNumber();
		else
			lastInsertedNumber = IncrementLastInsertedNumber();

		var id = MagicEnumCore<TSelf, TValue>.CreateMember(lastInsertedNumber ?? GetLastInsertedValue(), enforcedName!);
		return id;


		static Number<TValue>? IncrementLastInsertedNumber()
		{
			if (LastInsertedNumber is null)
				LastInsertedNumber = new();
			else
			{
				// ReSharper disable once RedundantSuppressNullableWarningExpression
				LastInsertedNumber!++;
			}
			
			return LastInsertedNumber;
		}
	}

	/// <summary>
	/// Creates a new enum member with the provided integral value.
	/// </summary>
	/// <param name="enforcedName">
	/// The name of the new member.
	/// Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten. 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected new static TSelf CreateMember(TValue value, [CallerMemberName] string? enforcedName = null)
	{
		if (IsInConcurrentState)
			lock (LockLastInsertedNumber) LastInsertedNumber = value;
		else
			LastInsertedNumber = value;

		return MagicEnumCore<TSelf, TValue>.CreateMember(value, enforcedName!);
	}
}