namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with a string as member value (with ordinal comparison)
/// Use <see cref="MagicStringEnum{TSelf}.CreateMember(string?)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicStringEnum<TSelf> : MagicEnumCore<TSelf, string>
	where TSelf : MagicStringEnum<TSelf>
{
	/// <inheritdoc cref="MagicEnumCore{TSelf,TValue}.CreateMember"/>
	public static TSelf CreateMember([CallerMemberName] string? name = null) 
		=> CreateMember(value: name!, name: name!);
	
	/// <inheritdoc cref="MagicEnumCore{TSelf,TValue}.GetOrCreateMember"/>
	public static TSelf GetOrCreateMember([CallerMemberName] string? name = null) 
		=> GetOrCreateMember(value: name!, name: name!);
}