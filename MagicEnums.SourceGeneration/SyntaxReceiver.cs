﻿using CodeChops.MagicEnums.SourceGeneration.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using CodeChops.MagicEnums.SourceGeneration.Extensions;

namespace CodeChops.MagicEnums.SourceGeneration;

internal static class SyntaxReceiver
{
	public const string InterfaceName		= "IMagicEnum";
	public const string InterfaceNamespace	= "CodeChops.MagicEnums.Core";
	public const string AttributeNamespace	= "CodeChops.MagicEnums.Attributes";
	public const string AttributeName		= "DiscoverableEnumMembers";

	internal static bool CheckIfIsProbablyEnum(SyntaxNode syntaxNode, CancellationToken cancellationToken)
	{
		if (syntaxNode is not AttributeSyntax attribute || attribute.ArgumentList is null || attribute.ArgumentList?.Arguments.Count > 1) return false;
		if (attribute.Parent?.Parent is not RecordDeclarationSyntax) return false;
		if (ExtractAttributeName(attribute.Name, cancellationToken) is not AttributeName and not $"{AttributeName}Attribute") return false;

		return true;


		static string? ExtractAttributeName(NameSyntax? name, CancellationToken cancellationToken)
		{
			while (name != null && !cancellationToken.IsCancellationRequested)
			{
				switch (name)
				{
					case IdentifierNameSyntax identifierName:
						return identifierName.Identifier.Text;

					case QualifiedNameSyntax qualifiedName:
						name = qualifiedName.Right;
						break;

					default:
						return null;
				}
			}

			return null;
		}
	}

	internal static EnumDefinition? TryGetEnumDefinition(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var attributeSyntax = (AttributeSyntax)context.Node;
		if (attributeSyntax.Parent?.Parent is not RecordDeclarationSyntax typeDeclaration) return null;
		if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken) is not INamedTypeSymbol type) return null;

		var enumDeclaration = TryGetDeclaration(typeDeclaration, type);
		return enumDeclaration;


		static EnumDefinition? TryGetDeclaration(RecordDeclarationSyntax typeDeclaration, INamedTypeSymbol type)
		{
			if (type is null || type.IsStatic || !type.IsRecord || !typeDeclaration.Modifiers.Any(m => m.ValueText == "partial")) return null;
			if (!type.HasAttribute(AttributeName, AttributeNamespace, out var attribute)) return null;
			if (!type.IsOrImplementsInterface(type => type.IsType(InterfaceName, InterfaceNamespace, isGenericType: true), out var interf)) return null;
			if (!interf.IsGeneric(typeParameterCount: 1, out var genericTypeArgument)) return null;

			var implicitDiscoverability = attribute?.ConstructorArguments.FirstOrDefault().Value is true;
			var filePath = typeDeclaration.SyntaxTree.FilePath;
			var data = new EnumDefinition(type, valueType: genericTypeArgument.Single(), implicitDiscoverability, filePath, typeDeclaration.Modifiers.ToFullString());

			return data;
		}
	}

	internal static bool CheckIfIsProbablyMemberInvokation(SyntaxNode syntaxNode)
	{
		if (syntaxNode is InvocationExpressionSyntax invocation)
		{
			var argumentCount = invocation.ArgumentList.Arguments.Count;
			if (argumentCount > 2) return false;

			if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return false;
			if (memberAccess.Name.Identifier.ValueText != SourceBuilder.GenerateMethodName) return false;
			if (memberAccess.Expression is not MemberAccessExpressionSyntax childMemberAccess) return false;
			if (childMemberAccess.Expression is not IdentifierNameSyntax) return false;
		}
		else if (syntaxNode is MemberAccessExpressionSyntax memberAccess && memberAccess.Parent is not InvocationExpressionSyntax)
		{
			if (memberAccess.Expression is not IdentifierNameSyntax) return false;
		}

		return true;
	}

	internal static EnumMember? TryGetMember(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		if (context.Node is InvocationExpressionSyntax invocation)
		{
			var argumentCount = invocation.ArgumentList.Arguments.Count;
			if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return null;
			if (memberAccess.Name.Identifier.ValueText != SourceBuilder.GenerateMethodName) return null;
			if (memberAccess.Expression is not MemberAccessExpressionSyntax childMemberAccess) return null;
			if (childMemberAccess.Expression is not IdentifierNameSyntax identifier) return null;

			var enumName = identifier.Identifier.ValueText;
			var memberName = childMemberAccess.Name.Identifier.ValueText;

			string? memberValue = null;
			if (argumentCount == 1)
			{
				if (invocation.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax memberValueLiteral) return null;
				memberValue = memberValueLiteral.Token.ValueText;
			}

			string? memberComment = null;
			if (argumentCount == 2)
			{
				if (invocation.ArgumentList.Arguments[1].Expression is not LiteralExpressionSyntax commentLiteral) return null;
				memberComment = commentLiteral?.Token.ValueText;
			}

			var filePath = invocation.SyntaxTree.FilePath;
			var startLinePosition = invocation.SyntaxTree.GetLineSpan(memberAccess.Span, cancellationToken).StartLinePosition;

			var member = GetMember(enumName, memberName.Trim('"'), memberValue, memberComment?.Trim('"'), isImplicitlyDiscovered: false, filePath, startLinePosition);
			return member;
		}
		else if (context.Node is MemberAccessExpressionSyntax memberAccess && memberAccess.Parent is not InvocationExpressionSyntax)
		{
			var memberName = memberAccess.Name.Identifier.ValueText;
			if (memberAccess.Expression is not IdentifierNameSyntax identifierName) return null;
			var enumName = identifierName.Identifier.ValueText;
			var filePath = memberAccess.SyntaxTree.FilePath;
			var startLinePosition = memberAccess.SyntaxTree.GetLineSpan(memberAccess.Span, cancellationToken).StartLinePosition;

			var member = GetMember(enumName, memberName.Trim('"'), value: null, comment: null, isImplicitlyDiscovered: true, filePath, startLinePosition);
			return member;
		}

		return null;

		static EnumMember? GetMember(string enumName, string name, string? value, string? comment, bool isImplicitlyDiscovered, string filePath, LinePosition startLinePosition)
		{
			var member = new EnumMember(enumName, name.Trim('"'), value, comment?.Trim('"'), isImplicitlyDiscovered, filePath, startLinePosition);

			return member;
		}
	}
}