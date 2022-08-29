﻿namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with an integer value. 
/// Use <see cref="MagicEnum{TSelf, TValue}.CreateMember(string?)"/> 
/// or <see cref="MagicEnum{TSelf, TValue}.CreateMember(TValue, string?)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicEnum<TSelf> : MagicEnum<TSelf, int> where TSelf : MagicEnum<TSelf>;

/// <summary>
/// An enum with an integral value.
/// Use <see cref="MagicEnum{TSelf, TValue}.CreateMember(string?)"/> 
/// or <see cref="MagicEnum{TSelf, TValue}.CreateMember(TValue, string?)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TValue">The integral type.</typeparam>
public abstract record MagicEnum<TSelf, TValue> : MagicEnumCore<TSelf, TValue>
	where TSelf : MagicEnum<TSelf, TValue>
	where TValue : struct, IComparable<TValue>, IEquatable<TValue>, IConvertible
{
	/// <summary>
	/// Used for auto-incrementing the value of a new member when no value has been provided (implicit value).
	/// </summary>
	private static Number<TValue>? LastNumber { get; set; }
	/// <summary>
	/// Locks the retrieval and incrementation of the last number.
	/// </summary>
	private static readonly object LockLastNumber = new();

	/// <summary>
	/// Creates a new enum member with an incremental value.
	/// </summary>
	/// <param name="name">
	/// The name of the new member.
	/// Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten. 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected static TSelf CreateMember([CallerMemberName] string? name = null)
	{
		return CreateMember(IncrementAndRetrieveLastNumber, name!);

		
		static TValue IncrementAndRetrieveLastNumber()
		{
			if (IsInConcurrentState)
				lock (LockLastNumber) return IncrementLastNumber();
			
			return IncrementLastNumber();
			
			
			static Number<TValue> IncrementLastNumber()
			{
				if (LastNumber is null)
					LastNumber = Number<TValue>.Zero;
				else
					LastNumber = Calculator<TValue>.Increment(LastNumber.Value);

				return LastNumber!.Value;
			}
		}
	}

	/// <summary>
	/// Creates a new enum member with the provided integral value.
	/// </summary>
	/// <param name="value">The value of the new member.</param>
	/// <param name="name">
	/// The name of the member.
	/// Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten. 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected static TSelf CreateMember(TValue value, [CallerMemberName] string? name = null) 
		=> CreateMember(() => SetAndRetrieveLastNumber(value), name!);

	/// <summary>
	/// Creates a new enum member with the provided integral value, or gets an existing member of the provided name.
	/// </summary>
	/// <param name="value">The value of the new member.</param>
	/// <param name="name">
	/// The name of the member.
	/// Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten. 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected static TSelf GetOrCreateMember(TValue value, [CallerMemberName] string name = null!) 
		=> GetOrCreateMember(() => SetAndRetrieveLastNumber(value), name);

	/// <summary>
	/// Sets and retrieves the last number (during a lock, if necessary).
	/// </summary>
	private static TValue SetAndRetrieveLastNumber(TValue value)
	{
		if (IsInConcurrentState)
			lock (LockLastNumber) LastNumber = value;
		else
			LastNumber = value;

		return value;
	}
}