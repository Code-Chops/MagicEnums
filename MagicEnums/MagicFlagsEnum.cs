namespace CodeChops.MagicEnums;

/// <summary>
/// A flags enum with integer values. 
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicFlagsEnum<TSelf> : MagicFlagsEnum<TSelf, int> 
	where TSelf : MagicFlagsEnum<TSelf>;

/// <summary>
/// A flags enum with integral values.
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
	
	/// <summary>
	/// Bit shifts the last inserted enum value and returns it.
	/// </summary>
	private static TValue GetBitShiftedLastInsertedNumber()
	{
		if (LastInsertedNumber is null) return Number<TValue>.Zero;
		if (LastInsertedNumber == Number<TValue>.Zero) return Calculator<TValue>.Increment(LastInsertedNumber.Value);
		return Calculator<TValue>.LeftShift(LastInsertedNumber.Value, 1);
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
	/// <param name="memberCreator">Optional: A function to construct subtypes without parameterless constructors.</param>
	/// <param name="name">
	/// The name of the new member.
	/// <b>Do not provide this parameter!</b>
	///<para>
	/// If not provided, the name of the caller of this method will be used as the name of the member.<br/>
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para>
	/// </param>
	/// <exception cref="InvalidOperationException">When a member with the same name already exists.</exception>
	/// <exception cref="ArgumentNullException">When the member name argument is null.</exception>
	protected static TMember CreateMember<TMember>(Func<TMember>? memberCreator = null, TValue? value = null, [CallerMemberName] string name = null!)
		where TMember : TSelf 
		=> CreateMember(
			valueCreator: value is null ? GetBitShiftedLastInsertedNumber : () => value.Value, 
			memberCreator: memberCreator,
			name: name);

	// Hides the magic enum core method in order to force saving the last inserted number.
	/// <inheritdoc cref="MagicEnumCore{TSelf,TValue}.CreateMember{TMember}"/>
	protected new static TMember CreateMember<TMember>(Func<TValue> valueCreator, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!)
		where TMember : TSelf
		=> MagicEnumCore<TSelf, TValue>.CreateMember(
			valueCreator: () => SetAndRetrieveLastInsertedNumber(valueCreator), 
			memberCreator: memberCreator, 
			name: name);
	
	#endregion
	
	#region GetOrCreateMember
	
	// Creates or retrieves a new enum member of the same type as the enum itself.
	/// <inheritdoc cref="GetOrCreateMember{TMember}(TValue, Func{TMember}?, string)"/>
	protected static TSelf GetOrCreateMember(TValue value, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> GetOrCreateMember<TSelf>(value, memberCreator, name);
	
	/// <summary>
	/// Creates a new enum member with the provided integral flags and returns it, or gets an existing member of the provided name.
	/// </summary>
	/// <param name="value">The value of the new member.</param>
	/// <param name="memberCreator">Optional: A function to construct subtypes without parameterless constructors.</param>
	/// <param name="name">
	/// The name of the new member.
	/// <b>Do not provide this parameter!</b>
	///<para>
	/// If not provided, the name of the caller of this method will be used as the name of the member.<br/>
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para>
	/// </param>
	/// <exception cref="ArgumentNullException">When the member name argument is null.</exception>
	protected static TMember GetOrCreateMember<TMember>(TValue value, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!)
		where TMember : TSelf
		=> GetOrCreateMember(
			valueCreator: () => SetAndRetrieveLastInsertedNumber(() => value), 
			memberCreator: memberCreator, 
			name: name);

	// Hides the magic enum core method in order to force saving the last inserted number. 
	/// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.CreateMember{TMember}"/>
	protected new static TMember GetOrCreateMember<TMember>(Func<TValue> valueCreator, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!)
		where TMember : TSelf
		=> MagicEnumCore<TSelf, TValue>.GetOrCreateMember(
			valueCreator: () => SetAndRetrieveLastInsertedNumber(valueCreator), 
			memberCreator: memberCreator, 
			name: name);
	
	#endregion

	/// <summary>
	/// Get the unique flags of the provided value.
	/// </summary>
	public static IEnumerable<TSelf> GetUniqueFlags(TValue flags)
		=> GetMembers().Where(member => member.HasFlag(flags));

	/// <summary>
	/// Returns true if the enum member contains the provided flag. 
	/// </summary>
	public bool HasFlag(TValue flag)
	{
		var uThis = Convert.ToUInt64(this.Value);
		var uFlag = Convert.ToUInt64(flag);
		
		return (uThis & uFlag) == uFlag; 
	}
}