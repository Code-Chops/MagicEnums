namespace CodeChops.MagicEnums.Attributes;

/// <summary>
/// Add this attribute to the enum record to make the enum members auto-discoverable. Which means that the members are automatically added to the enum member while referencing them.
/// <list type="bullet">
/// <item>Explicit discoverability: 
/// <para>This method has extra parameters to add a value and a comment to the enum member. Usage: <code>Invoking {Enum}.{Member}.CreateMember(...)</code></para></item>
/// <item>Implicit discoverability: 
/// <para>Disabled by default. Usage: <code>Invoking {Enum}.{NewMember}</code></para></item>
/// </list>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class DiscoverableEnumMembers : Attribute
{
	public bool ImplicitDiscoverability { get; }

	public DiscoverableEnumMembers(bool implicitDiscoverability = false)
	{
		this.ImplicitDiscoverability = implicitDiscoverability;
	}
}