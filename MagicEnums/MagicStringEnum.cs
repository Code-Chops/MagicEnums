namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with a string as member value (with ordinal comparison)
/// Use <see cref="MagicStringEnum{TSelf}.CreateMember(string?)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicStringEnum<TSelf> : MagicEnumCore<TSelf, string>
	where TSelf : MagicStringEnum<TSelf>
{
	/// <summary>
	/// Creates a new enum member and returns it.
	/// </summary>
	/// <param name="name">
	/// The name of the new member.
	/// <para>
	/// Warning: Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para> 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member with the same name already exists.</exception>
	protected static TSelf CreateMember([CallerMemberName] string? name = null) 
		=> CreateMember(valueCreator: () => name!, name: name!);
	
	/// <summary>
	/// Creates a new enum member if it does not exist and returns it. When it already exists, it returns the member with the same name.
	/// </summary>
	/// <param name="name">
	/// The name of the member.
	/// <para>
	/// Warning: Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para> 
	/// </param>
	/// <returns>The newly created member or an existing enum member with the same name.</returns>
	protected static TSelf GetOrCreateMember([CallerMemberName] string? name = null) 
		=> GetOrCreateMember(valueCreator: () => name!, name: name!);
}