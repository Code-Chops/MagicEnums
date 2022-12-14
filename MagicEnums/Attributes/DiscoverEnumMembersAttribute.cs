namespace CodeChops.MagicEnums.Attributes;

/// <summary>
/// Add this attribute to the enum record to make the enum members auto-discoverable. Which means that the members are automatically added to the enum member while referencing them.
/// <list type="bullet">
/// <item>Explicit discoverability: 
/// <para>This method has extra parameters to add a value and a comment to the enum member.</para>
/// Usage: <code>Invoking {Enum}.{Member}.CreateMember(...)</code>
/// </item>
/// <item>Implicit discoverability: 
/// <para>Disabled by default.</para>
/// Usage: <code>Invoking {Enum}.{NewMember}</code>
/// </item>
/// </list>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class DiscoverEnumMembers : Attribute
{
	public bool Implicitly { get; }

	public DiscoverEnumMembers(bool implicitly = false)
	{
		this.Implicitly = implicitly;
	}
}