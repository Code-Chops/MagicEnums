using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using CodeChops.MagicEnums.SourceGeneration.Entities;
using Microsoft.CodeAnalysis;

namespace CodeChops.MagicEnums.SourceGeneration;

[Generator]
public class EnumDefinitionDiscoverer : IIncrementalGenerator
{
	public const string InterfaceName = "IMagicEnum";
	public const string InterfaceNamespace = "CodeChops.MagicEnums.Core";
	public static bool IsRunning { get; set; } = true;
	internal static Dictionary<string, EnumDefinition> EnumDefinitionsByName { get; set; } = new();

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Get enum definitions
		var enumDefinitions = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (syntaxNode, ct) => SourceGenerator.CheckIfIsProbablyEnum(syntaxNode, ct),
				transform: static (context, ct) => SourceGenerator.TryGetEnumDefinition(context, ct))
			.Where(static declaration => declaration is not null)
			.Collect();

		context.RegisterSourceOutput(
			source: enumDefinitions,
			action: (spc, source) => GenerateFile(source!));	
	}

	private static void GenerateFile(ImmutableArray<EnumDefinition> enumDefinitions)
	{
		EnumDefinitionsByName = enumDefinitions.ToDictionary(definition => definition!.Name)!;
		IsRunning = false;
	}
}