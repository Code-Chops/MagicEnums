namespace CodeChops.MagicEnums.SourceGeneration.Entities;

internal record EnumMember
{
	public string Name { get; }
	public string? Value { get; }
	public string? Comment { get; }

	public EnumMember(string name, string? value, string? comment)
	{
		this.Name = String.IsNullOrWhiteSpace(name) ? "_IncorrectName_" : name;
		this.Value = String.IsNullOrWhiteSpace(value) ? null : value;
		this.Comment = String.IsNullOrWhiteSpace(comment) ? null : comment;
	}
}
