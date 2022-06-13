using Microsoft.CodeAnalysis;

namespace CodeChops.MagicEnums.SourceGeneration.Entities;

public record EnumDefinition
{
	public string Name { get; }
	public string? Namespace { get; }
	public string ValueTypeName { get; }
	public string ValueTypeNamespace { get; }
	public bool IsStringEnum { get; }
	public DiscoverabilityMode DiscoverabilityMode { get; }
	public string FilePath { get; }
	public string AccessModifier { get; }
	public List<EnumMember> AttributeMembers { get; }
	public bool IsStruct { get; }

	public EnumDefinition(INamedTypeSymbol type, string valueTypeName, string valueTypeNamespace, DiscoverabilityMode discoverabilityMode, string filePath, string accessModifier, List<EnumMember> attributeMembers)
	{
		this.Name = type.Name;

		var ns = type.ContainingNamespace?.ToDisplayString();
		this.Namespace = String.IsNullOrWhiteSpace(ns) ? null : ns;

		this.ValueTypeName = valueTypeName;
		this.ValueTypeNamespace = valueTypeNamespace;

		this.IsStringEnum = valueTypeName.Equals(nameof(String), StringComparison.OrdinalIgnoreCase);
		this.DiscoverabilityMode = discoverabilityMode;
		this.FilePath = filePath;
		this.AccessModifier = accessModifier.Replace("partial ", "").Replace("static ", "");

		this.AttributeMembers = attributeMembers;
		this.IsStruct = type.TypeKind == TypeKind.Struct;
	}
}