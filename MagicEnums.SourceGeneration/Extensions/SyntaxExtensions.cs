using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeChops.MagicEnums.SourceGeneration.Extensions;

internal static class SyntaxExtensions
{
	/// <summary>
	/// Checks if a name syntax has a specific attribute.
	/// </summary>
	public static bool HasAttributeName(this NameSyntax? name, string expectedName, CancellationToken cancellationToken)
	{
		var attributeName = name.ExtractAttributeName(cancellationToken);

		if (attributeName is null) return false;
		if (attributeName == expectedName) return true;
		
		var alternativeAttributeName = attributeName.EndsWith("Attribute")
		   ? attributeName.Substring(0, attributeName.Length - "Attribute".Length)
		   : $"{attributeName}Attribute";

		return alternativeAttributeName == expectedName;
	}

	/// <summary>
	/// Extracts the attribute name from the name syntax.
	/// </summary>
	/// <returns>The attribute name</returns>
	public static string? ExtractAttributeName(this NameSyntax? name, CancellationToken cancellationToken)
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