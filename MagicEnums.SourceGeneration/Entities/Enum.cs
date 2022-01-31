using Microsoft.CodeAnalysis.Text;
using System.Collections.Concurrent;

namespace CodeChops.MagicEnums.SourceGeneration.Entities;

internal record Enum
{
	public ConcurrentDictionary<(string FilePath, LinePosition LinePosition), EnumMember> MemberByKeys { get; } = new();

	public Enum(string filePath, LinePosition linePosition, EnumMember member)
	{
		this.MemberByKeys.TryAdd((filePath, linePosition), member);
	}
}