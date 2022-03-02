using CodeChops.MagicEnums.SourceGeneration.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using CodeChops.MagicEnums.SourceGeneration.Extensions;

namespace CodeChops.MagicEnums.SourceGeneration.SyntaxReceivers;

internal class EnumDefinitionSyntaxReceiver
{
	/// <summary>
	/// The predicate for every node that is probably an enum definition which has discoverable members or attributes.
	/// </summary>
	internal static bool CheckIfIsProbablyEnumDefinition(SyntaxNode syntaxNode, CancellationToken cancellationToken)
	{
		if (syntaxNode is not AttributeSyntax attribute || attribute.ArgumentList is null || attribute.ArgumentList.Arguments.Count == 0 || attribute.ArgumentList.Arguments.Count > 3) return false;
		if (attribute.Parent?.Parent is not RecordDeclarationSyntax) return false;

		var hasAttributeName = attribute.Name.HasAttributeName(SourceGenerator.DiscoverableAttributeName, cancellationToken)
			|| attribute.Name.HasAttributeName(SourceGenerator.MemberAttributeName, cancellationToken);

		return hasAttributeName;
	}

	/// <summary>
	/// Checks if the node is an enum definition which has discoverable members or attributes.
	/// </summary>
	/// <returns>The enum definition. Or null if not applicable for this node.</returns>
	internal static EnumDefinition? GetEnumDefinition(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var attributeSyntax = (AttributeSyntax)context.Node;
		if (attributeSyntax.Parent?.Parent is not RecordDeclarationSyntax typeDeclaration) return null;
		if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken) is not INamedTypeSymbol type) return null;

		if (type is null || type.IsStatic || !type.IsRecord || !typeDeclaration.Modifiers.Any(m => m.ValueText == "partial")) return null;

		if (!type.IsOrImplementsInterface(type => type.IsType(SourceGenerator.InterfaceName, SourceGenerator.InterfaceNamespace, isGenericType: true), out var interf)) return null;
		if (!interf.IsGeneric(typeParameterCount: 1, out var genericTypeArgument)) return null;

		var hasDiscoverableAttribute = type.HasAttribute(SourceGenerator.DiscoverableAttributeName, SourceGenerator.AttributeNamespace, out var discoverableAttribute);
		var hasAttributeMembers = type.HasAttributes(SourceGenerator.MemberAttributeName, SourceGenerator.AttributeNamespace, out var attributeMemberDataList);

		if (!hasDiscoverableAttribute && !hasAttributeMembers) return null;

		var attributeMembers = attributeMemberDataList.Select(data => new EnumMember(data));

		var hasImplicitDiscoverability = discoverableAttribute?.ConstructorArguments.FirstOrDefault().Value is true;

		var discoverabilityMode = DiscoverabilityMode.None;
		
		if (hasDiscoverableAttribute) discoverabilityMode = hasImplicitDiscoverability ? DiscoverabilityMode.Implicit : DiscoverabilityMode.Explicit;

		var filePath = typeDeclaration.SyntaxTree.FilePath;

		var definition = new EnumDefinition(
			type: type, 
			valueType: genericTypeArgument.Single(),
			discoverabilityMode: discoverabilityMode, 
			filePath: filePath, 
			accessModifier: typeDeclaration.Modifiers.ToFullString(),
			attributeMembers: attributeMembers.ToList());

		return definition;
	}
}