using Microsoft.CodeAnalysis;
using CodeChops.MagicEnums.SourceGeneration.Entities;
using System.Collections.Immutable;

namespace CodeChops.MagicEnums.SourceGeneration;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
	private Dictionary<string, EnumDefinition> EnumDefinitionsByName { get; set; } = new();

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Get enum definitions
		var enumDefinitions = context.SyntaxProvider
		.CreateSyntaxProvider(
			predicate: static (syntaxNode, ct)	=> SyntaxReceiver.CheckIfIsProbablyEnum(syntaxNode, ct),
			transform: static (context, ct)		=> SyntaxReceiver.TryGetEnumDefinition(context, ct))
		.Where(static declaration => declaration is not null)
		.Collect();

		context.RegisterSourceOutput(
			source: enumDefinitions,
			action: (spc, source) => AddEnumDefinitions(source!));

		var memberInvokations = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (syntaxNode, ct)	=> SyntaxReceiver.CheckIfIsProbablyMemberInvokation(syntaxNode),
				transform: static (context, ct)		=> SyntaxReceiver.TryGetMember(context, ct))
			.Where(static declaration => declaration is not null)
			.Collect();

		context.RegisterSourceOutput(
			source: memberInvokations,
			action: (context, members) => SourceBuilder.CreateCode(context, members!, EnumDefinitionsByName!));


		void AddEnumDefinitions(ImmutableArray<EnumDefinition> enumDefinitions)
		{
			EnumDefinitionsByName = enumDefinitions.ToDictionary(definition => definition!.Name)!;
		}
	}
}