namespace CodeChops.MagicEnums.Attributes;

/// <summary>
/// Generates a magic enum member.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class EnumMember : Attribute
{
	public string Name { get; }
	public object? Value { get; }
	public string? Comment { get; }

	public EnumMember(string name, object? value = null, string? comment = null)
	{
		this.Name = String.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
		this.Value = value;
		this.Comment = String.IsNullOrWhiteSpace(comment) ? null : comment;
	}
}