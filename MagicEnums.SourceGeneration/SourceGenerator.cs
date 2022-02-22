using Microsoft.CodeAnalysis;
using CodeChops.MagicEnums.SourceGeneration.Entities;
using System.Collections.Immutable;
using CodeChops.MagicEnums.SourceGeneration.SourceBuilders;
using CodeChops.MagicEnums.SourceGeneration.SyntaxReceivers;

namespace CodeChops.MagicEnums.SourceGeneration;

/// <summary>
/// Generates newly enum members by discovering them from an implicit or explicit invocation.
/// This way no manual enum member definition is needed anymore.
/// This discovery will only work on enums with the correct attribute <see cref="DiscoverableAttributeName"/>.
/// </summary>
[Generator]
public class SourceGenerator : IIncrementalGenerator
{
	internal const string InterfaceName						= "IMagicEnum";
	internal const string InterfaceNamespace				= "CodeChops.MagicEnums.Core";
	internal const string AttributeNamespace				= "CodeChops.MagicEnums.Attributes";
	internal const string DiscoverableAttributeName			= "DiscoverableEnumMembers";
	internal const string CreateInternalCopyAttributeName	= "CloneAsInternal";
	internal const string GenerateMethodName				= "GenerateMember";

	private Dictionary<string, EnumDefinition>? EnumDefinitionsByName { get; set; }

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		this.FindAndRegisterEnumDefinitions(context);
		this.RegisterEnumSourceCode(context);
	}

	/// <summary>
	/// Finds the enum definitions and stores the enum definitions in the property for later use.
	/// </summary>
	private void FindAndRegisterEnumDefinitions(IncrementalGeneratorInitializationContext context)
	{
		// Get the enum definitions.
		var enumDefinitions = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (syntaxNode, ct)	=> DiscoverableMembersSyntaxReceiver.CheckIfIsProbablyEnumDefinition(syntaxNode, ct),
				transform: static (context, ct)		=> DiscoverableMembersSyntaxReceiver.GetEnumDefinition(context, ct))
			.Where(static definition => definition is not null)
			.Collect();

		// Store the enum definitions in the property for later use.
		context.RegisterSourceOutput(
			source: enumDefinitions,
			action: (source, definitions) => AddEnumDefinitions(definitions!));
		
		void AddEnumDefinitions(ImmutableArray<EnumDefinition> enumDefinitions)
		{
			this.EnumDefinitionsByName = enumDefinitions.ToDictionary(definition => definition!.Name);
		}
	}

	/// <summary>
	/// Retrieves the probable enum member invocations.
	/// </summary>
	private IncrementalValueProvider<ImmutableArray<DiscoveredEnumMember?>> GetProbableEnumMemberInvocations(IncrementalGeneratorInitializationContext context)
	{
		var memberInvokations = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (syntaxNode, ct)	=> DiscoverableMembersSyntaxReceiver.CheckIfIsProbablyEnumMemberInvocation(syntaxNode),
				transform: (context, ct)			=> DiscoverableMembersSyntaxReceiver.GetProbablyNewEnumMember(context, EnumDefinitionsByName, ct))
			.Where(static member => member is not null)
			.Collect();

		return memberInvokations;
	}

	/// <summary>
	/// Generates the enum source code.
	/// </summary>
	private void RegisterEnumSourceCode(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterSourceOutput(
			source: this.GetProbableEnumMemberInvocations(context),
			action: (context, members) => EnumSourceBuilder.CreateSource(context, members!, EnumDefinitionsByName!));
	}
}