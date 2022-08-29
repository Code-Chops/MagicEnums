namespace CodeChops.MagicEnums;

/// <summary>
/// A flags enum with an integer flags. 
/// Use <see cref="MagicFlagsEnum{TSelf, TValue}.CreateMember(string?)"/> 
/// or <see cref="MagicEnum{TSelf, TValue}.CreateMember(TValue, string?)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicFlagsEnum<TSelf> : MagicFlagsEnum<TSelf, int> where TSelf : MagicFlagsEnum<TSelf>;

/// <summary>
/// A flags enum with an integral flags.
/// Use <see cref="MagicFlagsEnum{TSelf, TValue}.CreateMember(string?)"/> 
/// or <see cref="MagicEnum{TSelf, TValue}.CreateMember(TValue, string?)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TValue">The integral type.</typeparam>
public abstract record MagicFlagsEnum<TSelf, TValue> : MagicEnumCore<TSelf, TValue>
	where TSelf : MagicFlagsEnum<TSelf, TValue>
	where TValue : struct, IComparable<TValue>, IEquatable<TValue>, IConvertible
{
	/// <summary>
	/// Used for incremental bit-shifting the flags of a new member when no flags has been provided (implicit flags).
	/// </summary>
	private static Number<TValue>? LastNumber { get; set; }
	
	/// <summary>
	/// Locks the retrieval and incrementation of the last number.
	/// </summary>
	private static readonly object LockLastNumber = new();

	/// <summary>
	/// Creates a new enum member with an bit-shifted incremental flags.
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
				lock (LockLastNumber) return BitShiftLastNumber();

			return BitShiftLastNumber();
			
			
			static Number<TValue> BitShiftLastNumber()
			{
				if (LastNumber is null) LastNumber = Number<TValue>.Zero;
				else if (LastNumber == Number<TValue>.Zero) LastNumber = Calculator<TValue>.Increment(LastNumber.Value);
				else LastNumber = Calculator<TValue>.LeftShift(LastNumber.Value, 1);

				return LastNumber!.Value;
			}
		}
	}

	/// <summary>
	/// Creates a new enum member with the provided integral flags.
	/// </summary>
	/// <param name="value">The flags of the new member.</param>
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
	/// Creates a new enum member with the provided integral flags, or gets an existing member of the provided name.
	/// </summary>
	/// <param name="value">The flags of the new member.</param>
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
	
	public static IEnumerable<TSelf> GetUniqueFlags(TValue flags)
		=> GetMembers().Where(member => member.HasFlag(flags));

	public bool HasFlag(TValue flag)
	{
		var uThis = Convert.ToUInt64(this.Value);
		var uFlag = Convert.ToUInt64(flag);
		
		return (uThis & uFlag) == uFlag; 
	}
}