using Microsoft.CodeAnalysis;

namespace CodeChops.MagicEnums.SourceGeneration.Entities;

internal record EnumDeclaration
{
	public string EnumName { get; }
	public string? Namespace { get; }
	public ITypeSymbol ValueType { get; }
	public bool IsStringEnum { get; }
	public bool ImplicitDiscoverabilityIsEnabled { get; }
	public string FilePath { get; }
	public string AccessModifier { get; }

	public EnumDeclaration(INamedTypeSymbol type, ITypeSymbol valueType, bool implicitDiscoverability, string filePath, string accessModifier)
	{
		this.EnumName = type.Name;

		var ns = type.ContainingNamespace?.ToDisplayString();
		this.Namespace = String.IsNullOrWhiteSpace(ns) ? null : ns;

		this.ValueType = valueType;
		this.IsStringEnum = valueType.Name.Equals(nameof(String), StringComparison.OrdinalIgnoreCase);
		this.ImplicitDiscoverabilityIsEnabled = implicitDiscoverability;
		this.FilePath = filePath;
		this.AccessModifier = accessModifier.Replace("partial ", "").Replace("static ", "");
	}
}