using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;

namespace CodeChops.MagicEnums.SourceGeneration.Entities;

internal record EnumDefinition
{
	public string Name { get; set; }
	public string? Namespace { get; set; }
	public string ValueTypeName { get; set; }
	public string ValueTypeNamespace { get; set; }
	public bool IsStringEnum { get; set; }
	public bool ImplicitDiscoverabilityIsEnabled { get; set; }
	public string FilePath { get; set; }
	public string AccessModifier { get; set; }

	public EnumDefinition(INamedTypeSymbol type, ITypeSymbol valueType, bool implicitDiscoverability, string filePath, string accessModifier)
	{
		this.Name = type.Name;

		var ns = type.ContainingNamespace?.ToDisplayString();
		this.Namespace = String.IsNullOrWhiteSpace(ns) ? null : ns;

		this.ValueTypeName = valueType.Name;
		this.ValueTypeNamespace = valueType.ContainingNamespace.ToDisplayString();

		this.IsStringEnum = valueType.Name.Equals(nameof(String), StringComparison.OrdinalIgnoreCase);
		this.ImplicitDiscoverabilityIsEnabled = implicitDiscoverability;
		this.FilePath = filePath;
		this.AccessModifier = accessModifier.Replace("partial ", "").Replace("static ", "");
	}
	public EnumDefinition()
	{

	}
}