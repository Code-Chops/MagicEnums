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
	/// <param name="memberCreator">Optional: A function to construct subtypes without parameterless constructors.</param>
	/// <param name="name">
	/// The name of the new member.
	/// <b>Do not provide this parameter!</b>
	///<para>
	/// If not provided, the name of the caller of this method will be used as the name of the member.<br/>
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para>
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="InvalidOperationException">When a member with the same name already exists.</exception>
	protected static TMember CreateMember<TMember>(TValue value, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!)
		where TMember : TSelf
		=> CreateMember(valueCreator: () => value, memberCreator, name: name);

	#endregion
	
	#region GetOrCreateMember
	
	// Creates or retrieves a new enum member of the same type as the enum itself.
	/// <inheritdoc cref="GetOrCreateMember{TMember}(string, TValue, Func{TMember}?)"/>
	protected static TSelf GetOrCreateMember(string name, TValue value, Func<TSelf>? memberCreator = null)
		=> GetOrCreateMember<TSelf>(name: name, value, memberCreator);
	
	/// <summary>
	/// Creates a new enum member if it does not exist and returns it. If it already exists, it returns the member with the same name.
	/// </summary>
	/// <param name="name">The name of the new member.</param>
	/// <param name="value">The value of the member.</param>
	/// <param name="memberCreator">Optional: A function to construct subtypes without parameterless constructors.</param>
	/// <returns>The newly created member or an existing enum member with the same name.</returns>
	protected static TMember GetOrCreateMember<TMember>(string name, TValue value, Func<TMember>? memberCreator = null) 
		where TMember : TSelf
		=> GetOrCreateMember(name: name, () => value, memberCreator);
	
	#endregion
}