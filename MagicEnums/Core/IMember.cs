namespace CodeChops.MagicEnums.Core.Members;

public interface IMember<TValue>
	where TValue : notnull
{
	/// <summary>
	/// The name of the enum member.
	/// </summary>
	string Name { get; }
	
	/// <summary>
	/// The value of the enum member.
	/// </summary>
	TValue Value { get; }
}