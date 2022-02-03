using Microsoft.CodeAnalysis;
using CodeChops.MagicEnums.SourceGeneration.Entities;
using System.Collections.Immutable;

namespace CodeChops.MagicEnums.SourceGeneration;

/// <summary>
/// Generates newly enum members by discovering them from an implicit or explicit invocation.
/// This way no manual enum member definition is needed anymore.
/// This discovery will only work on enums with the correct attribute <see cref="AttributeName"/>.
/// </summary>
[Generator]
public class SourceGenerator : IIncrementalGenerator
{
	internal const string InterfaceName			= "IMagicEnum";
	internal const string InterfaceNamespace	= "CodeChops.MagicEnums.Core";
	internal const string AttributeNamespace	= "CodeChops.MagicEnums.Attributes";
	internal const string AttributeName			= "DiscoverableEnumMembers";
	internal const string GenerateMethodName	= "GenerateMember";

	private Dictionary<string, EnumDefinition> EnumDefinitionsByName { get; set; } = new();

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Get the enum definitions.
		var enumDefinitions = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (syntaxNode, ct)	=> SyntaxReceiver.CheckIfIsProbablyEnumDefinition(syntaxNode, ct),
				transform: static (context, ct)		=> SyntaxReceiver.GetEnumDefinition(context, ct))
			.Where(static definition => definition is not null)
			.Collect();

		// Store the enum definitions in the property for later use.
		context.RegisterSourceOutput(
			source: enumDefinitions,
			action: (source, definitions) => AddEnumDefinitions(definitions!));

		// Get the probable enum member invocations.
		var memberInvokations = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (syntaxNode, ct)	=> SyntaxReceiver.CheckIfIsProbablyEnumMemberInvocation(syntaxNode),
				transform: static (context, ct)		=> SyntaxReceiver.GetProbablyNewEnumMember(context, ct))
			.Where(static member => member is not null)
			.Collect();

		// Generate the enum source code.
		context.RegisterSourceOutput(
			source: memberInvokations,
			action: (context, members) => SourceBuilder.CreateSource(context, members!, EnumDefinitionsByName!));


		void AddEnumDefinitions(ImmutableArray<EnumDefinition> enumDefinitions)
		{
			EnumDefinitionsByName = enumDefinitions.ToDictionary(definition => definition!.Name)!;
		}
	}
}