using CodeChops.MagicEnums.SourceGeneration.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeChops.MagicEnums.SourceGeneration.SyntaxReceivers;

internal static class DiscoverableMembersSyntaxReceiver
{
	/// <summary>
	/// The predicate for every node that is probably an enum member invocation.
	/// </summary>
	internal static bool CheckIfIsProbablyEnumMemberInvocation(SyntaxNode syntaxNode)
	{
		// Explicit enum member invocation.
		if (syntaxNode is InvocationExpressionSyntax invocation)
		{
			var argumentCount = invocation.ArgumentList.Arguments.Count;
			if (argumentCount > 2) return false;

			if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return false;
			if (memberAccess.Name.Identifier.ValueText != SourceGenerator.GenerateMethodName) return false;
			if (memberAccess.Expression is not MemberAccessExpressionSyntax childMemberAccess) return false;
			if (childMemberAccess.Expression is not IdentifierNameSyntax) return false;

			return true;
		}
		// Implicit enum member invocation.
		else if (syntaxNode is MemberAccessExpressionSyntax memberAccess && memberAccess.Parent is not InvocationExpressionSyntax)
		{
			if (memberAccess.Expression is not IdentifierNameSyntax) return false;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Gets the probably enum member based on the enum member invocation at the node.
	/// </summary>
	/// <returns>The probably new enum member. Or null if not applicable for this node.</returns>
	internal static DiscoveredEnumMember? GetProbablyDiscoveredEnumMember(GeneratorSyntaxContext context, Dictionary<string, EnumDefinition>? enumDefinitionsByName, CancellationToken cancellationToken)
	{
		// Explicit enum member invocation.
		if (context.Node is InvocationExpressionSyntax invocation)
		{
			var argumentCount = invocation.ArgumentList.Arguments.Count;
			if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return null;
			if (memberAccess.Name.Identifier.ValueText != SourceGenerator.GenerateMethodName) return null;
			if (memberAccess.Expression is not MemberAccessExpressionSyntax childMemberAccess) return null;
			if (childMemberAccess.Expression is not IdentifierNameSyntax identifier) return null;

			var enumName = identifier.Identifier.ValueText;
			var memberName = childMemberAccess.Name.Identifier.ValueText;

			string? memberValue = null;
			if (argumentCount >= 1)
			{
				if (invocation.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax memberValueLiteral) return null;
				memberValue = memberValueLiteral.Token.ValueText;
			}

			string? memberComment = null;
			if (argumentCount == 2)
			{
				if (invocation.ArgumentList.Arguments[1].Expression is not LiteralExpressionSyntax commentLiteral) return null;
				memberComment = commentLiteral.Token.ValueText;
			}

			var filePath = invocation.SyntaxTree.FilePath;
			var startLinePosition = invocation.SyntaxTree.GetLineSpan(memberAccess.Span, cancellationToken).StartLinePosition;

			var member = GetMember(enumName, memberName.Trim('"'), memberValue, memberComment?.Trim('"'), discoverabilityMode: DiscoverabilityMode.Explicit, filePath, startLinePosition, enumDefinitionsByName);
			return member;
		}
		// Implicit enum member invocation.
		else if (context.Node is MemberAccessExpressionSyntax { Parent: not InvocationExpressionSyntax } memberAccess)
		{
			var memberName = memberAccess.Name.Identifier.ValueText;
			if (memberAccess.Expression is not IdentifierNameSyntax identifierName) return null;
			var enumName = identifierName.Identifier.ValueText;
			var filePath = memberAccess.SyntaxTree.FilePath;
			var startLinePosition = memberAccess.SyntaxTree.GetLineSpan(memberAccess.Span, cancellationToken).StartLinePosition;

			var member = GetMember(enumName, memberName.Trim('"'), value: null, comment: null, discoverabilityMode: DiscoverabilityMode.Implicit, filePath, startLinePosition, enumDefinitionsByName);
			return member;
		}

		return null;


		static DiscoveredEnumMember? GetMember(string enumName, string name, string? value, string? comment, DiscoverabilityMode discoverabilityMode, string filePath, LinePosition startLinePosition, Dictionary<string, EnumDefinition>? enumDefinitionsByName)
		{
			if (enumDefinitionsByName is not null && enumDefinitionsByName.Count > 0)
			{
				if (!enumDefinitionsByName.TryGetValue(enumName, out var definition)) return null;
				if (definition.DiscoverabilityMode != discoverabilityMode) return null;
			}

			var member = new DiscoveredEnumMember(enumName, name.Trim('"'), value, comment?.Trim('"'), discoverabilityMode, filePath, startLinePosition);
			return member;
		}
	}
}