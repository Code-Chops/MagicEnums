using Microsoft.CodeAnalysis.Text;

namespace CodeChops.MagicEnums.SourceGeneration.Entities;

internal record DiscoveredEnumMember : EnumMember
{
	public string EnumName { get; }
	public DiscoverabilityMode DiscoverabilityMode { get; }
	public string FilePath { get; } 
	public LinePosition LinePosition { get; }

	public DiscoveredEnumMember(string enumName, string name, string? value, string? comment, DiscoverabilityMode discoverabilityMode, string filePath, LinePosition linePosition)
		: base(name, value, comment)
	{
		if (discoverabilityMode == DiscoverabilityMode.None)
		{
			throw new ArgumentException($"Member {name} of enum {enumName} should be implicity or explicitly discovered. File path: {filePath}. Line position: {linePosition}.");
		}

		this.EnumName = enumName;
		this.DiscoverabilityMode = discoverabilityMode;
		this.FilePath = filePath;
		this.LinePosition = linePosition;
	}
}