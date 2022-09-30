namespace CodeChops.MagicEnums.SourceGeneration.SyntaxReceivers;

internal sealed class EnumDefinitionSyntaxReceiver
{
	/// <summary>
	/// The predicate for every node that is probably an enum definition which has discoverable members or attributes.
	/// </summary>
	internal static bool CheckIfIsProbablyEnumDefinition(SyntaxNode syntaxNode, CancellationToken cancellationToken)
	{
		if (syntaxNode is not AttributeSyntax attribute || attribute.ArgumentList is null || attribute.ArgumentList.Arguments.Count > 3) return false;
		if (attribute.Parent?.Parent is not RecordDeclarationSyntax) return false;

		var hasAttributeName = attribute.Name.HasAttributeName(MagicEnumSourceGenerator.DiscoverableAttributeName, cancellationToken)
			|| attribute.Name.HasAttributeName(MagicEnumSourceGenerator.MemberAttributeName, cancellationToken);

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
		if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken) is not { } type) return null;

		if (type.IsStatic || !type.IsRecord || !typeDeclaration.Modifiers.Any(m =>  m.IsKind(SyntaxKind.PartialKeyword))) return null;

		if (!type.IsOrInheritsClass(type => type.IsType(MagicEnumSourceGenerator.CoreName, MagicEnumSourceGenerator.CoreNamespace, isGenericType: true), out var baseClass)) return null;
		if (!baseClass.IsGeneric(typeParameterCount: 2, out var genericTypeArgument)) return null;

		var hasDiscoverableAttribute = type.HasAttribute(MagicEnumSourceGenerator.DiscoverableAttributeName, MagicEnumSourceGenerator.AttributeNamespace, out var discoverableAttribute);
		var hasAttributeMembers = type.HasAttributes(MagicEnumSourceGenerator.MemberAttributeName, MagicEnumSourceGenerator.AttributeNamespace, out var attributeMemberDataList);

		if (!hasDiscoverableAttribute && !hasAttributeMembers) return null;

		var attributeMembers = attributeMemberDataList.Select(data => new EnumMember(data));

		var hasImplicitDiscoverability = discoverableAttribute?.ConstructorArguments.FirstOrDefault().Value is true;

		var discoverabilityMode = DiscoverabilityMode.None;
		
		if (hasDiscoverableAttribute) discoverabilityMode = hasImplicitDiscoverability ? DiscoverabilityMode.Implicit : DiscoverabilityMode.Explicit;

		var filePath = typeDeclaration.SyntaxTree.FilePath;

		var valueType = genericTypeArgument.Skip(1).FirstOrDefault();

		var definition = new EnumDefinition(
			type: type,
			valueTypeNameIncludingGenerics: valueType?.Name ?? "Int32",
			valueTypeNamespace: valueType?.ContainingNamespace.IsGlobalNamespace ?? false
				? null
				: valueType?.ContainingNamespace.ToDisplayString() ?? "System",
			discoverabilityMode: discoverabilityMode,
			filePath: filePath,
			accessModifier: typeDeclaration.Modifiers.ToFullString(),
			attributeMembers: attributeMembers.ToList());

		return definition;
	}
}