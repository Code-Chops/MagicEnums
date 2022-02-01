using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using CodeChops.MagicEnums.SourceGeneration.Entities;
using CodeChops.MagicEnums.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Text.Json;

namespace CodeChops.MagicEnums.SourceGeneration;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
	public const string InterfaceName = "IMagicEnum";
	public const string InterfaceNamespace = "CodeChops.MagicEnums.Core";
	public const string AttributeNamespace = "CodeChops.MagicEnums.Attributes";
	public const string AttributeName = "DiscoverableEnumMembers";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		//Debugger.Launch();

		Dictionary<string, EnumDefinition> enumDefinitionsByName = null;
		try
		{
			var json = File.ReadAllText("D:\\EnumDefinitionsByName.txt");
			enumDefinitionsByName = JsonSerializer.Deserialize<Dictionary<string, EnumDefinition>>(json);
			if ((enumDefinitionsByName?.Count ?? 0) == 0) return;
		}
		catch
		{
			return;
		}
		

		var memberInvokations = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (syntaxNode, ct)	=> CheckIfIsProbablyMemberInvokation(syntaxNode),
				transform: (context, ct)		=> TryGetMember(context, enumDefinitionsByName!, ct))
			.Where(static declaration => declaration is not null)
			.Collect();

		context.RegisterSourceOutput(
			source: memberInvokations, 
			action: (context, members) => SourceBuilder.CreateCode(context, members!, enumDefinitionsByName!));
	}

	internal static bool CheckIfIsProbablyEnum(SyntaxNode syntaxNode, CancellationToken cancellationToken)
	{
		//Debugger.Launch();
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

	private static bool CheckIfIsProbablyMemberInvokation(SyntaxNode syntaxNode)
	{
		try
		{
			if (syntaxNode is InvocationExpressionSyntax invocation)
			{
				var argumentCount = invocation.ArgumentList.Arguments.Count;
				if (argumentCount > 2) return false;

				if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return false;
				if (memberAccess.Name.Identifier.ValueText != SourceBuilder.GenerateMethodName) return false;
				if (memberAccess.Expression is not MemberAccessExpressionSyntax childMemberAccess) return false;
				if (childMemberAccess.Expression is not IdentifierNameSyntax identifier) return false;
			}
			else if (syntaxNode is MemberAccessExpressionSyntax memberAccess && memberAccess.Parent is not InvocationExpressionSyntax)
			{
				if (memberAccess.Expression is not IdentifierNameSyntax identifierName) return false;
			}
			else
			{
				return false;
			}
		}
#pragma warning disable CS0168 // Variable is declared but never used
#pragma warning disable IDE0059 // Unnecessary assignment of a value
		catch (Exception e)
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore CS0168 // Variable is declared but never used
		{
			Debugger.Launch();
			return false;
		}

		return true;
	}

	private static EnumMember? TryGetMember(GeneratorSyntaxContext context, Dictionary<string, EnumDefinition> enumDefinitionByName, CancellationToken cancellationToken)
	{
		try
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
					if (invocation.ArgumentList.Arguments[1].Expression is not LiteralExpressionSyntax memberValueLiteral) return null;
					memberValue = memberValueLiteral.Token.ValueText;
				}

				string? memberComment = null;
				if (argumentCount == 2)
				{
					if (invocation.ArgumentList.Arguments[2].Expression is not LiteralExpressionSyntax commentLiteral) return null;
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
		}
#pragma warning disable CS0168 // Variable is declared but never used
		catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
		{
			Debugger.Launch();
		}

		return null;

		EnumMember? GetMember(string enumName, string name, string? value , string? comment, bool isImplicitlyDiscovered, string filePath, LinePosition startLinePosition)
		{
			if (!enumDefinitionByName.TryGetValue(enumName, out var definition)) return null;
			if (!definition.ImplicitDiscoverabilityIsEnabled && isImplicitlyDiscovered) return null;

			var member = new EnumMember(enumName, name.Trim('"'), value, comment?.Trim('"'), isImplicitlyDiscovered, filePath, startLinePosition);

			return member;
		}
	}
}