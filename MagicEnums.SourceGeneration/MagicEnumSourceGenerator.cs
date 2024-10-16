using System.Diagnostics;
using CodeChops.MagicEnums.SourceGeneration.SourceBuilders;
using CodeChops.MagicEnums.SourceGeneration.SyntaxReceivers;

namespace CodeChops.MagicEnums.SourceGeneration;

/// <summary>
/// Generates newly enum members by discovering them from an implicit or explicit invocation.
/// This way manual enum member definitions are not needed anymore.
/// This discovery will only work on enums with the correct attribute <see cref="DiscoverableAttributeName"/>.
/// </summary>
[Generator]
public class MagicEnumSourceGenerator : IIncrementalGenerator
{
	internal const string GenerateMethodName				= "CreateMember";
	internal const string CoreName							= "MagicEnumCore";
	internal const string CoreNamespace						= "CodeChops.MagicEnums.Core";
	internal const string AttributeNamespace				= "CodeChops.MagicEnums.Attributes";
	internal const string MemberAttributeName				= "EnumMember";
	internal const string DiscoverableAttributeName			= "DiscoverEnumMembers";

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
				predicate: EnumDefinitionSyntaxReceiver.CheckIfIsProbablyEnumDefinition,
				transform: EnumDefinitionSyntaxReceiver.GetEnumDefinition)
			.Where(static definition => definition is not null)
			.Collect();

		// Store the enum definitions in the property for later use.
		context.RegisterSourceOutput(
			source: enumDefinitions,
			action: AddEnumDefinitions!);


		void AddEnumDefinitions(SourceProductionContext context, ImmutableArray<EnumDefinition> enumDefinitions)
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
		var memberInvocations = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: DiscoverableMembersSyntaxReceiver.CheckIfIsProbablyEnumMemberInvocation,
				transform: (context, ct) => DiscoverableMembersSyntaxReceiver.GetProbablyDiscoveredEnumMember(context, this.EnumDefinitionsByName, ct))
			.Where(static member => member is not null)
			.Collect();

		return memberInvocations;
	}

	/// <summary>
	/// Generates the enum source code.
	/// </summary>
	private void RegisterEnumSourceCode(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterSourceOutput(
			source: this.GetProbableDiscoveredEnumMembers(context).Combine(context.AnalyzerConfigOptionsProvider),
			action: (context, members) => EnumSourceBuilder.CreateSource(context, members.Left!, this.EnumDefinitionsByName!, members.Right));
	}
}