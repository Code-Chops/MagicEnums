using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeChops.MagicEnums.SourceGeneration.Entities;

internal record CloneAsInternalClass
{
	public RecordDeclarationSyntax Declaration { get; }
	public string? TargetNamespace { get; }
	public string AttributeName { get; }
	public List<string> Usings { get; }

	public CloneAsInternalClass(RecordDeclarationSyntax declaration, string? targetNamespace, string attributeName, List<string> usings)
	{
		this.Declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));
		this.TargetNamespace = targetNamespace;
		this.AttributeName = attributeName ?? throw new ArgumentNullException(nameof(attributeName));
		this.Usings = usings;
	}
}