namespace CodeChops.MagicEnums.Attributes;

/// <summary>
/// Add this attribute to the enum record to make the enum members auto-discoverable. 
/// Invoking [EnumRecord].CreateAndUse() will automatically add an enum member and consume it.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class DiscoverableEnumMembersAttribute : Attribute
{
}