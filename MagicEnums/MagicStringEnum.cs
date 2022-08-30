namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with a string as member value (with ordinal comparison)
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicStringEnum<TSelf> : MagicEnumCore<TSelf, string>
	where TSelf : MagicStringEnum<TSelf>
{
	#region CreateMember
	
	// Creates a new enum member of the same type as the enum itself.
	/// <inheritdoc cref="CreateMember{TMember}(string?, Func{TMember}?, string)"/>
	protected static TSelf CreateMember(string? value = null, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> CreateMember<TSelf>(value, memberCreator, name);
	
	/// <summary>
	/// Creates a new enum member and returns it.
	/// </summary>
	/// <param name="value">The value of the new member. Do not provide this value, in order to use the name of the member as value.</param>
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
	protected static TMember CreateMember<TMember>(string? value = null, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!) 
		where TMember : TSelf
		=> CreateMember(
			valueCreator: () => value ?? name, 
			memberCreator: memberCreator, 
			name: name);
	
	#endregion
	
	#region GetOrCreateMember
	
	/// <inheritdoc cref="GetOrCreateMember{TMember}(string?, Func{TMember}?, string)"/>
	protected static TSelf GetOrCreateMember(string? value = null!, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> GetOrCreateMember<TSelf>(value ?? name, memberCreator, name);

	/// <summary>
	/// Creates a new enum member if it does not exist and returns it. When it already exists, it returns the member with the same name.
	/// </summary>
	/// <param name="value">The value of the new member. Do not provide this value, in order to use the name of the member as value.</param>
	/// <param name="memberCreator">Provide this value in order to add enum members that have extra properties.</param>
	/// <param name="name">
	/// The name of the member.
	/// <para>
	/// Warning: Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para> 
	/// </param>
	/// <returns>The newly created member or an existing enum member with the same name.</returns>
	protected static TSelf GetOrCreateMember<TMember>(string? value = null!, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!)
		where TMember : TSelf
		=> GetOrCreateMember(valueCreator: () => value ?? name, name: name!);

	#endregion
}