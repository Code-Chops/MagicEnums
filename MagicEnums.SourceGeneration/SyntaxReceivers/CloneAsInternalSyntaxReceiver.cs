using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using CodeChops.MagicEnums.SourceGeneration.Extensions;
using CodeChops.MagicEnums.SourceGeneration.Entities;

namespace CodeChops.MagicEnums.SourceGeneration;

internal static class CloneAsInternalSyntaxReceiver
{
	/// <summary>
	/// The predicate for a record that has the correct attribute.
	/// </summary>
	internal static bool HasCloneAsInternalAttribute(SyntaxNode syntaxNode, CancellationToken cancellationToken)
	{
		if (syntaxNode is not AttributeSyntax attribute) return false;
		if (attribute.Parent?.Parent is not RecordDeclarationSyntax) return false;

		var hasAttributeName = attribute.Name.HasAttributeName(SourceGenerator.CreateInternalCopyAttributeName, cancellationToken);
		return hasAttributeName;
	}

	/// <summary>
	/// Get the definition
	/// </summary>
	/// <param name="context"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	internal static CloneAsInternalClass? GetCloneAsInternalDefinition(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var attributeSyntax = (AttributeSyntax)context.Node;
		if (attributeSyntax.Parent?.Parent is not RecordDeclarationSyntax typeDeclaration) return null;
		if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken) is not INamedTypeSymbol type) return null;

		if (type is null || !typeDeclaration.Modifiers.Any(m => m.ValueText == "partial") || !typeDeclaration.Modifiers.Any(m => m.ValueText == "public")) return null;
		if (!type.HasAttribute(SourceGenerator.CreateInternalCopyAttributeName, SourceGenerator.AttributeNamespace, out var attribute)) return null;

		var descendantNodes = context.Node.SyntaxTree
			.GetRoot(cancellationToken)
			.DescendantNodes();

		var targetNamespace = attribute?.ConstructorArguments.Single().Value?.ToString()
			?? descendantNodes
				.OfType<BaseNamespaceDeclarationSyntax>()
				.SingleOrDefault()
				?.Name
				.ToFullString() + ".Internal";
		
		var usings = descendantNodes
			.OfType<UsingDirectiveSyntax>()
			.Select(node => node.ToFullString())
			.ToList();

		var definition = new CloneAsInternalClass(
			declaration: typeDeclaration,
			targetNamespace: targetNamespace,
			attributeName: attributeSyntax.Name.ToString(),
			usings: usings);

		return definition;
	}
}