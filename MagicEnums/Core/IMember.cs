namespace CodeChops.MagicEnums.Core;

public interface IMember<out TValue>
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