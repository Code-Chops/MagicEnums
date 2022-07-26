﻿namespace CodeChops.MagicEnums.SourceGeneration.Entities;

public record EnumDefinition : IEnumEntity
{
	public string Identifier { get; }
	public string Name { get; }
	public string? Namespace { get; }
	public string ValueTypeName { get; }
	public string? ValueTypeNamespace { get; }
	public DiscoverabilityMode DiscoverabilityMode { get; }
	public string FilePath { get; }
	public string AccessModifier { get; }
	public List<EnumMember> AttributeMembers { get; }
	public bool IsStruct { get; }

	public EnumDefinition(INamedTypeSymbol type, string valueTypeNameIncludingGenerics, string? valueTypeNamespace, DiscoverabilityMode discoverabilityMode, 
		string filePath, string accessModifier, IEnumerable<EnumMember> attributeMembers)
		: this(
			name: type.Name, 
			enumNamespace: type.ContainingNamespace.IsGlobalNamespace 
				? null 
				: type.ContainingNamespace.ToDisplayString(),
			valueTypeNameIncludingGenerics: valueTypeNameIncludingGenerics, 
			valueTypeNamespace: valueTypeNamespace, 
			discoverabilityMode: discoverabilityMode,
			filePath: filePath, 
			accessModifier: accessModifier, 
			attributeMembers: attributeMembers, 
			isStruct: type.TypeKind == TypeKind.Struct)
	{
	}

	public EnumDefinition(string name, string? enumNamespace, string valueTypeNameIncludingGenerics, string? valueTypeNamespace, DiscoverabilityMode discoverabilityMode, 
		string filePath, string accessModifier, IEnumerable<EnumMember> attributeMembers, bool isStruct)
	{
		this.Name = name;
		this.Namespace = enumNamespace;

		this.ValueTypeName = valueTypeNameIncludingGenerics;
		this.ValueTypeNamespace = valueTypeNamespace;

		this.DiscoverabilityMode = discoverabilityMode;
		this.FilePath = filePath;
		this.AccessModifier = accessModifier.Replace("partial ", "").Replace("static ", "");

		this.AttributeMembers = attributeMembers as List<EnumMember> ?? attributeMembers.ToList();
		this.IsStruct = isStruct;
		
		var genericParameterIndex = valueTypeNameIncludingGenerics.IndexOf('<');
		var valueTypeNameWithoutGenerics = genericParameterIndex <= 0 ? valueTypeNameIncludingGenerics : valueTypeNameIncludingGenerics.Substring(0, genericParameterIndex);
		this.Identifier = $"{this.Namespace ?? $"{this.Namespace}."}{valueTypeNameWithoutGenerics}";
	}
}