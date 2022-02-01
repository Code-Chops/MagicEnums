using Microsoft.CodeAnalysis.Text;

namespace CodeChops.MagicEnums.SourceGeneration.Entities;

internal record EnumMember
{
	public string EnumName { get; }
	public string Name { get; }
	public string? Value { get; }
	public string? Comment { get; }
	public bool IsImplicitlyDiscovered { get; }
	public string FilePath { get; } 
	public LinePosition LinePosition { get; }

	public EnumMember(string enumName, string name, string? value, string? comment, bool isImplicitlyDiscovered, string filePath, LinePosition linePosition)
	{
		this.EnumName = enumName;
		this.Name = String.IsNullOrWhiteSpace(name) ? "_IncorrectName_" : name;
		this.Value = String.IsNullOrWhiteSpace(value) ? null : value;
		this.Comment = String.IsNullOrWhiteSpace(comment) ? null : comment;
		this.IsImplicitlyDiscovered = isImplicitlyDiscovered;
		this.FilePath = filePath;
		this.LinePosition = linePosition;
	}
}