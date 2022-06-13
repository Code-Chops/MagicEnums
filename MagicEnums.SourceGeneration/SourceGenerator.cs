using Microsoft.CodeAnalysis;
using CodeChops.MagicEnums.SourceGeneration.Entities;
using System.Collections.Immutable;
using CodeChops.MagicEnums.SourceGeneration.SourceBuilders;
using CodeChops.MagicEnums.SourceGeneration.SyntaxReceivers;

namespace CodeChops.MagicEnums.SourceGeneration;

/// <summary>
/// Generates newly enum members by discovering them from an implicit or explicit invocation.
/// This way manual enum member definitions are not needed anymore.
/// This discovery will only work on enums with the correct attribute <see cref="DiscoverableAttributeName"/>.
/// </summary>
[Generator]
public class SourceGenerator : IIncrementalGenerator
{
	public const string GenerateMethodName					= "CreateMember";
	internal const string InterfaceName						= "IMagicEnumCore";
	internal const string InterfaceNamespace				= "CodeChops.MagicEnums.Core";
	internal const string AttributeNamespace				= "CodeChops.MagicEnums.Attributes";
	internal const string MemberAttributeName				= "EnumMember";
	internal const string DiscoverableAttributeName			= "DiscoverableEnumMembers";
	internal const string CreateInternalCopyAttributeName	= "CloneAsInternal";

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
				predicate: static (syntaxNode, ct)	=> EnumDefinitionSyntaxReceiver.CheckIfIsProbablyEnumDefinition(syntaxNode, ct),
				transform: static (context, ct)		=> EnumDefinitionSyntaxReceiver.GetEnumDefinition(context, ct))
			.Where(static definition => definition is not null)
			.Collect();

		// Store the enum definitions in the property for later use.
		context.RegisterSourceOutput(
			source: enumDefinitions,
			action: (source, definitions) => AddEnumDefinitions(definitions!));
		
		void AddEnumDefinitions(ImmutableArray<EnumDefinition> enumDefinitions)
		{
			this.EnumDefinitionsByName = enumDefinitions
				.GroupBy(definition => definition.Name)
				.ToDictionary(grouping => grouping.Key, grouping => grouping.First());
		}
	}

	/// <summary>
	/// Retrieves the probable discovered enum members.
	/// </summary>
	private IncrementalValueProvider<ImmutableArray<DiscoveredEnumMember?>> GetProbableDiscoveredEnumMembers(IncrementalGeneratorInitializationContext context)
	{
		var memberInvokations = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (syntaxNode, ct)	=> DiscoverableMembersSyntaxReceiver.CheckIfIsProbablyEnumMemberInvocation(syntaxNode),
				transform: (context, ct)			=> DiscoverableMembersSyntaxReceiver.GetProbablyDiscoveredEnumMember(context, EnumDefinitionsByName, ct))
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
			source: this.GetProbableDiscoveredEnumMembers(context),
			action: (context, members) => EnumSourceBuilder.CreateSource(context, members!, EnumDefinitionsByName!));
	}
}