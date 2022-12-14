namespace CodeChops.MagicEnums.SourceGeneration.Models;

internal sealed record EnumDefinition
{
	public string Name { get; }
	public string? Namespace { get; }
	public string ValueTypeName { get; }
	public string? ValueTypeNamespace { get; }
	public DiscoverabilityMode DiscoverabilityMode { get; }
	public string FilePath { get; }
	public string AccessModifier { get; }
	public List<EnumMember> AttributeMembers { get; }
	public bool IsStruct { get; }
	public bool IsNumberEnum { get; }

	public bool IsStringEnum => this is { ValueTypeName: "String", ValueTypeNamespace: "System" };
	
	public EnumDefinition(INamedTypeSymbol type, string valueTypeNameIncludingGenerics, string? valueTypeNamespace, DiscoverabilityMode discoverabilityMode, 
		string filePath, string accessModifier, IEnumerable<EnumMember> attributeMembers, bool isNumberEnum)
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
			isStruct: type.TypeKind == TypeKind.Struct,
			isNumberEnum)
	{
	}

	public EnumDefinition(string name, string? enumNamespace, string valueTypeNameIncludingGenerics, string? valueTypeNamespace, DiscoverabilityMode discoverabilityMode, 
		string filePath, string accessModifier, IEnumerable<EnumMember> attributeMembers, bool isStruct, bool isNumberEnum)
	{
		this.Name = name;
		this.Namespace = enumNamespace;

		this.ValueTypeName = valueTypeNameIncludingGenerics;
		this.ValueTypeNamespace = valueTypeNamespace;

		this.DiscoverabilityMode = discoverabilityMode;
		this.FilePath = filePath;
		this.AccessModifier = accessModifier.Replace("partial ", "").Replace("static ", "").Trim();

		this.AttributeMembers = attributeMembers as List<EnumMember> ?? attributeMembers.ToList();
		this.IsStruct = isStruct;
		this.IsNumberEnum = isNumberEnum;
	}
}