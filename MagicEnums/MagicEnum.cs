namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with an integer value. 
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicEnum<TSelf> : MagicEnum<TSelf, int> 
	where TSelf : MagicEnum<TSelf>;

/// <summary>
/// An enum with an integral value.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TValue">The integral type.</typeparam>
public abstract record MagicEnum<TSelf, TValue> : MagicEnumCore<TSelf, TValue>
	where TSelf : MagicEnum<TSelf, TValue>
	where TValue : struct, IComparable<TValue>, IEquatable<TValue>, IConvertible
{
	#region LastInsertedNumber
	
	/// <summary>
	/// The value of the latest inserted enum member (starts with -1).
	/// Used for auto-incrementing the value of a new member when no value has been provided (implicit value).
	/// </summary>
	private static Number<TValue>? LastInsertedNumber { get; set; }
	
	/// <summary>
	/// Locks the retrieval and incrementation of the last inserted number.
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
	/// <param name="value">The value of the new member. If not provided, the last inserted enum value will be incremented and used.</param>
	/// <param name="memberCreator">Provide this value in order to add enum members that have extra properties.</param>
	/// <param name="name">
	/// The name of the new member.
	/// <b>Do not provide this parameter.</b>
	///<para>
	/// If not provided, the name of the caller of this method will be used as the name of the member.<br/>
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para>
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected static TMember CreateMember<TMember>(Func<TMember>? memberCreator = null, TValue? value = null, [CallerMemberName] string name = null!)
		where TMember :  TSelf 
		=> (TMember)CreateMember(
			valueCreator: value is null ? GetIncrementedLastInsertedNumber : () => value.Value,
			memberCreator: memberCreator,
			name: name);

	private static TValue GetIncrementedLastInsertedNumber()
	{
		return LastInsertedNumber is null
			? Number<TValue>.Zero
			: Calculator<TValue>.Increment(LastInsertedNumber.Value);
	}
	
	// Creates a new member with a value creator which saves the last inserted number ('overrides' the core method).
	/// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.CreateMember{TMember}"/>
	protected static TSelf CreateMember(Func<TValue> valueCreator, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> MagicEnumCore<TSelf, TValue>.CreateMember(valueCreator: () => SetAndRetrieveLastInsertedNumber(valueCreator), memberCreator, name);

	#endregion
	
	#region GetOrCreateMember

	/// <inheritdoc cref="GetOrCreateMember{TMember}(TValue, Func{TMember}?, string)"/>
	protected static TSelf GetOrCreateMember(TValue value, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> GetOrCreateMember<TSelf>(value, memberCreator, name);
	
	/// <summary>
	/// Creates a new enum member with the provided integral value, or gets an existing member of the provided name.
	/// </summary>
	/// <param name="value">The value of the new member.</param>
	/// <param name="memberCreator">Provide this value in order to add enum members that have extra properties.</param>
	/// <param name="name">
	/// The name of the new member.
	/// <b>Do not provide this parameter.</b>
	///<para>
	/// If not provided, the name of the caller of this method will be used as the name of the member.<br/>
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para>
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected static TSelf GetOrCreateMember<TMember>(TValue value, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!) 
		where TMember : TSelf
		=> GetOrCreateMember(() => SetAndRetrieveLastInsertedNumber(() => value), memberCreator, name);

	/// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.CreateMember{TMember}"/>
	protected new static TSelf GetOrCreateMember(Func<TValue> valueCreator, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> MagicEnumCore<TSelf, TValue>.GetOrCreateMember(valueCreator: () => SetAndRetrieveLastInsertedNumber(valueCreator), memberCreator, name);

	#endregion
}