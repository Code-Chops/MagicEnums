namespace CodeChops.MagicEnums.Attributes;

/// <summary>
/// Generates a magic enum for a struct.
/// </summary>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public class StructEnum : Attribute
{
	public bool IsInternal { get; }

	public StructEnum(bool isInternal = false)
	{
		this.IsInternal = isInternal;
	}
}