namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with a custom type as member value.
/// Use <see cref="CreateMember(TValue, Func{TSelf}, string)"/> to create a member.
/// </summary>
public abstract record MagicCustomEnum<TSelf, TValue> : MagicEnumCore<TSelf, TValue> 
	where TSelf : MagicCustomEnum<TSelf, TValue> 
	where TValue : struct, IEquatable<TValue>, IComparable<TValue>
{
	#region CreateMember
	
	// Creates a new enum member of the same type as the enum itself.
	/// <inheritdoc cref="CreateMember{TMember}(TValue, Func{TMember}?, string)"/>
	protected static TSelf CreateMember(TValue value, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> CreateMember<TSelf>(value, memberCreator, name);
	
	/// <summary>
	/// Creates a new enum member and returns it.
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
	/// <exception cref="ArgumentException">When a member with the same name already exists.</exception>
	protected static TMember CreateMember<TMember>(TValue value, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!)
		where TMember : TSelf
		=> CreateMember(valueCreator: () => value, memberCreator, name: name);

	// Creates a new member with a value creator ('overrides' the core method).
	/// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.CreateMember{TMember}"/>
	protected new static TMember CreateMember<TMember>(Func<TValue> valueCreator, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!)
		where TMember : TSelf
		=> MagicEnumCore<TSelf, TValue>.CreateMember(valueCreator: valueCreator, memberCreator, name);

	#endregion
	
	#region GetOrCreateMember
	
	/// <inheritdoc cref="GetOrCreateMember{TMember}(TValue, Func{TMember}?, string)"/>
	protected static TSelf GetOrCreateMember(TValue value, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> GetOrCreateMember<TSelf>(value, memberCreator, name);
	
	/// <summary>
	/// Creates a new enum member if it does not exist and returns it. When it already exists, it returns the member with the same name.
	/// </summary>
	/// <param name="value">The value of the member.</param>
	/// <param name="memberCreator">Provide this value in order to add enum members that have extra properties.</param>
	/// <param name="name">
	/// The name of the new member.
	/// <b>Do not provide this parameter.</b>
	///<para>
	/// If not provided, the name of the caller of this method will be used as the name of the member.<br/>
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para>
	/// </param>
	/// <returns>The newly created member or an existing enum member with the same name.</returns>
	protected static TSelf GetOrCreateMember<TMember>(TValue value, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!) 
		where TMember : TSelf
		=> GetOrCreateMember(() => value, memberCreator, name);
	
	/// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.CreateMember{TMember}"/>
	protected new static TSelf GetOrCreateMember(Func<TValue> valueCreator, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> MagicEnumCore<TSelf, TValue>.GetOrCreateMember(valueCreator: valueCreator, memberCreator, name);
	
	#endregion
}