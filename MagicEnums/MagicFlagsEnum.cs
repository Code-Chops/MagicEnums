namespace CodeChops.MagicEnums;

/// <summary>
/// A flags enum with an integer flags. 
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicFlagsEnum<TSelf> : MagicFlagsEnum<TSelf, int> 
	where TSelf : MagicFlagsEnum<TSelf>;

/// <summary>
/// A flags enum with an integral flags.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TValue">The integral type.</typeparam>
public abstract record MagicFlagsEnum<TSelf, TValue> : MagicEnumCore<TSelf, TValue>
	where TSelf : MagicFlagsEnum<TSelf, TValue>
	where TValue : struct, IComparable<TValue>, IEquatable<TValue>, IConvertible
{
	#region LastInsertedNumber

	/// <summary>
	/// The value of the latest inserted enum member (starts with 0).
	/// Used for incremental bit-shifting the flags of a new member when no flags has been provided (implicit flags).
	/// </summary>
	// ReSharper disable once StaticMemberInitializerReferesToMemberBelow
	private static Number<TValue>? LastInsertedNumber { get; set; }
	
	/// <summary>
	/// Locks the retrieval and incrementation of the last number.
	/// </summary>
	private static readonly object LockLastInsertedNumber = new();

	/// <summary>
	/// Sets and retrieves the last number (during a lock, if necessary).
	/// </summary>
	private static TValue SetAndRetrieveLastInsertedNumber(Func<TValue> valueCreator)
	{
		if (IsInConcurrentState)
		{
			lock (LockLastInsertedNumber)
			{
				LastInsertedNumber = valueCreator();
				return LastInsertedNumber.Value;
			}
		}
		
		LastInsertedNumber = valueCreator();
		return LastInsertedNumber.Value;
	}
	
	#endregion
	
	#region CreateMember
	
	// Creates and returns a new enum member of the same type as the enum itself.
	/// <inheritdoc cref="CreateMember{TMember}(Func{TMember}?, TValue?, string)"/>
	protected static TSelf CreateMember(TValue? value = null, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> CreateMember(memberCreator, value, name);

	/// <summary>
	/// Creates a new enum member and returns it.
	/// </summary>
	/// <param name="value">The value of the new member. If not provided, the last inserted enum value will be bit shifted and used.</param>
	/// <param name="memberCreator">Provide this value in order to add enum members that have extra properties.</param>
	/// <param name="name">
	/// The name of the new member.
	/// <b>Do not provide this parameter.</b>
	///<para>
	/// If not provided, the name of the caller of this method will be used as the name of the member.<br/>
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para>
	/// </param>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected static TSelf CreateMember<TMember>(Func<TMember>? memberCreator = null, TValue? value = null, [CallerMemberName] string name = null!)
		where TMember : TSelf 
		=> CreateMember(
			valueCreator: value is null ? GetBitShiftedLastInsertedNumber : () => value.Value, 
			memberCreator: memberCreator,
			name: name);

	private static TValue GetBitShiftedLastInsertedNumber()
	{
		if (LastInsertedNumber is null) return Number<TValue>.Zero;
		if (LastInsertedNumber == Number<TValue>.Zero) return Calculator<TValue>.Increment(LastInsertedNumber.Value);
		return Calculator<TValue>.LeftShift(LastInsertedNumber.Value, 1);
	}

	// Creates and returns a new member with a value creator which saves the last inserted number ('overrides' the core method)..
	/// <inheritdoc cref="MagicEnumCore{TSelf,TValue}.CreateMember{TMember}"/>
	protected new static TMember CreateMember<TMember>(Func<TValue> valueCreator, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!)
		where TMember : TSelf
		=> MagicEnumCore<TSelf, TValue>.CreateMember(valueCreator: () => SetAndRetrieveLastInsertedNumber(valueCreator), memberCreator, name);
	
	#endregion
	
	#region GetOrCreateMember
	
	/// <inheritdoc cref="GetOrCreateMember{TMember}(TValue, Func{TMember}?, string)"/>
	protected static TSelf GetOrCreateMember(TValue value, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> GetOrCreateMember<TSelf>(value, memberCreator, name);
	
	/// <summary>
	/// Creates a new enum member with the provided integral flags and returns it, or gets an existing member of the provided name.
	/// </summary>
	/// <param name="value">The flags of the new member.</param>
	/// <param name="memberCreator">Provide this value in order to add enum members that have extra properties.</param>
	/// <param name="name">
	/// The name of the new member.
	/// <b>Do not provide this parameter.</b>
	///<para>
	/// If not provided, the name of the caller of this method will be used as the name of the member.<br/>
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para>
	/// </param>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected static TSelf GetOrCreateMember<TMember>(TValue value, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!)
		where TMember : TSelf
		=> GetOrCreateMember(() => SetAndRetrieveLastInsertedNumber(() => value), memberCreator, name);

	/// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.CreateMember{TMember}"/>
	protected new static TSelf GetOrCreateMember(Func<TValue> valueCreator, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> MagicEnumCore<TSelf, TValue>.GetOrCreateMember(valueCreator: () => SetAndRetrieveLastInsertedNumber(valueCreator), memberCreator, name);
	
	#endregion

	public static IEnumerable<TSelf> GetUniqueFlags(TValue flags)
		=> GetMembers().Where(member => member.HasFlag(flags));

	public bool HasFlag(TValue flag)
	{
		var uThis = Convert.ToUInt64(this.Value);
		var uFlag = Convert.ToUInt64(flag);
		
		return (uThis & uFlag) == uFlag; 
	}
}