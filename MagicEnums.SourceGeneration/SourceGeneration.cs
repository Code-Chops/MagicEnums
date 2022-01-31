using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Concurrent;
using CodeChops.MagicEnums.SourceGeneration.Entities;
using CodeChops.MagicEnums.SourceGeneration.Extensions;

namespace CodeChops.MagicEnums.SourceGeneration;

[Generator]
public class SourceGeneration : ISourceGenerator
{
	public const string AttributeNamespace	= "CodeChops.MagicEnums.Attributes";
	public const string AttributeName		= "DiscoverableEnumMembersAttribute";
	public const string GenerateMethodName	= "GenerateMember";
	public const string InterfaceName		= "IMagicEnum";
	public const string InterfaceNamespace	= "CodeChops.MagicEnums.Core";

	internal ConcurrentDictionary<string, Entities.Enum> EnumDataByNames { get; } = new(StringComparer.OrdinalIgnoreCase);
	internal ConcurrentDictionary<string, EnumDeclaration> EnumDeclarationByNames { get; } = new(StringComparer.OrdinalIgnoreCase);
	internal List<string> Errors { get; } = new();
	internal SourceBuilder SourceBuilder { get; } = new();

	public void Initialize(GeneratorInitializationContext context)
	{
		context.RegisterForSyntaxNotifications(() => new SyntaxReceiver(this));
	}

	public void Execute(GeneratorExecutionContext context)
	{
		try
		{
			foreach (var declarationByName in this.EnumDeclarationByNames)
			{
				var enumCode = this.SourceBuilder.CreateEnum(declaration: declarationByName.Value, this.EnumDataByNames);

				context.AddSource($"{declarationByName.Key}.g.cs", SourceText.From(enumCode, Encoding.UTF8));
			}
		}
		catch (Exception e)
		{
			context.ReportDiagnostic(
				id: "ESG1",
				title: "Unknown error",
				description: e.Message.ToString(),
				severity: DiagnosticSeverity.Error);
		}
	}
}