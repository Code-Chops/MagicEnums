using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using CodeChops.MagicEnums.SourceGeneration.Extensions;
using CodeChops.MagicEnums.SourceGeneration.Entities;

namespace CodeChops.MagicEnums.SourceGeneration;

public class SyntaxReceiver : ISyntaxContextReceiver
{
	private SourceGeneration Generator { get; }

	public SyntaxReceiver(SourceGeneration generator)
	{
		this.Generator = generator;
	}

	public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
	{
		try
		{
			if (context.Node is TypeDeclarationSyntax typeDeclarationSyntax)
			{
				var type = context.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax);

				if (type is null || type.IsStatic || !type.IsRecord || !typeDeclarationSyntax.Modifiers.Any(m => m.ValueText == "partial")) return;
				if (!type.HasAttribute(SourceGeneration.AttributeName, SourceGeneration.AttributeNamespace, out var attribute)) return;

				if (!type.IsOrImplementsInterface(type => type.IsType(SourceGeneration.InterfaceName, SourceGeneration.InterfaceNamespace, isGenericType: true), out var interf))
				{
					return;
				}

				if (!interf.IsGeneric(typeParameterCount: 1, out var genericTypeArgument)) return;

				var implicitDiscoverability = attribute?.ConstructorArguments.FirstOrDefault().Value is true;

				var filePath = typeDeclarationSyntax.SyntaxTree.FilePath;
				var data = new EnumDeclaration(type, valueType: genericTypeArgument.Single(), implicitDiscoverability, filePath, typeDeclarationSyntax.Modifiers.ToFullString());
				this.Generator.EnumDeclarationByNames.TryAdd(type.Name, data);
			}

			else if (context.Node is InvocationExpressionSyntax invocation)
			{
				if (this.Generator.EnumDeclarationByNames.IsEmpty) return;

				var argumentCount = invocation.ArgumentList.Arguments.Count;
				if (argumentCount > 2) return;

				if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return;
				if (memberAccess.Name.Identifier.ValueText != SourceGeneration.GenerateMethodName) return;
				if (memberAccess.Expression is not MemberAccessExpressionSyntax childMemberAccess) return;
				if (childMemberAccess.Expression is not IdentifierNameSyntax identifier) return;
				
				var enumName = identifier.Identifier.ValueText;
				var memberName = childMemberAccess.Name.Identifier.ValueText;

				string? memberValue = null;
				if (argumentCount == 1)
				{
					if (invocation.ArgumentList.Arguments[1].Expression is not LiteralExpressionSyntax memberValueLiteral) return;
					memberValue = memberValueLiteral.Token.ValueText;
				}

				string? memberComment = null;
				if (argumentCount == 2)
				{
					if (invocation.ArgumentList.Arguments[2].Expression is not LiteralExpressionSyntax commentLiteral) return;
					memberComment = commentLiteral?.Token.ValueText;
				}

				var filePath = invocation.SyntaxTree.FilePath;
				var startLinePosition = invocation.SyntaxTree.GetLineSpan(memberAccess.Span).StartLinePosition;

				AddMember(enumName, memberName, memberValue, memberComment, requiresImplicitDiscoverability: false, filePath, startLinePosition);
			}

			else if (context.Node is MemberAccessExpressionSyntax memberAccess && memberAccess.Parent is not InvocationExpressionSyntax)
			{
				if (this.Generator.EnumDeclarationByNames.IsEmpty) return;

				var memberName = memberAccess.Name.Identifier.ValueText;
				if (memberAccess.Expression is not IdentifierNameSyntax identifierName) return;
				var enumName = identifierName.Identifier.ValueText;
				var filePath = memberAccess.SyntaxTree.FilePath;
				var startLinePosition = memberAccess.SyntaxTree.GetLineSpan(memberAccess.Span).StartLinePosition;

				AddMember(enumName, memberName, memberValue: null, memberComment: null, requiresImplicitDiscoverability: true, filePath, startLinePosition);
			}
		}
#pragma warning disable CS0168 // Variable is declared but never used
		catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
		{
			Debugger.Launch();
		}

		void AddMember(string enumName, string memberName, string? memberValue, string? memberComment, bool requiresImplicitDiscoverability, string filePath, LinePosition startLinePosition)
		{
			if (!this.Generator.EnumDeclarationByNames.TryGetValue(enumName, out var declaration)) return;
			if (!declaration.ImplicitDiscoverabilityIsEnabled && requiresImplicitDiscoverability) return;

			var member = new EnumMember(memberName.Trim('"'), memberValue, memberComment?.Trim('"'));
			if (!this.Generator.EnumDataByNames.TryGetValue(enumName, out var enumData))
			{
				var data = new Entities.Enum(filePath, startLinePosition, member);
				this.Generator.EnumDataByNames.TryAdd(enumName, data);
			}
			else
			{
				var key = (filePath, startLinePosition);
				enumData.MemberByKeys.TryRemove(key, out _);
				enumData.MemberByKeys.TryAdd(key, member);
			}
		}
	}
}