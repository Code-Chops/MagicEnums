using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices.ComTypes;

namespace CodeChops.MagicEnums.SourceGeneration.Entities;

internal record EnumDefinition
{
	public string Name { get; }
	public string? Namespace { get; }
	public string ValueTypeName { get; }
	public string ValueTypeNamespace { get; }
	public bool IsStringEnum { get; }
	public bool ImplicitDiscoverabilityIsEnabled { get; }
	public string FilePath { get; }
	public string AccessModifier { get; }

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
}