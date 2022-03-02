﻿using System.Collections.Immutable;
using System.Text;
using CodeChops.MagicEnums.SourceGeneration.Entities;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Text.RegularExpressions;

namespace CodeChops.MagicEnums.SourceGeneration.SourceBuilders;

internal static class CloneAsInternalSourceBuilder
{
	/// <summary>
	/// Creates a copy of the record and only changes its access modifier from 'public' to 'internal'.
	/// It also changed changes the namespace to the provided target namespace. 
	/// If the target namespace is not provided, it will fall back to {CurrentNamespace}.Internal.
	/// </summary>
	public static void CreateSource(SourceProductionContext context, ImmutableArray<CloneAsInternalClass> definitions)
	{
		if (definitions.IsDefaultOrEmpty) return;

		var definitionsByNames = definitions
			.GroupBy(definition => definition!.Declaration.Identifier.ValueText)
			.ToDictionary(definitionByName => definitionByName.Key, definitionByName => definitionByName.ToList());

		foreach (var definition in definitions)
		{
			var code = CreateCodeFromDefinition(definition);

			var definitionsWithSameName = definitionsByNames[definition!.Declaration.Identifier.ValueText];
			var numberString = definitionsWithSameName.Count > 1 
				? definitionsWithSameName.IndexOf(definition).ToString() 
				: "";

			context.AddSource($"{definition!.Declaration.Identifier.ValueText}Internal{numberString}.g.cs", SourceText.From(code, Encoding.UTF8));
		}


		static string CreateCodeFromDefinition(CloneAsInternalClass definition)
		{
			var declaration = new Regex(Regex.Escape("public"))
				.Replace(definition.Declaration.ToFullString(), "internal", 1)
				.Replace($"[{definition.AttributeName}]", "")
				.Trim();

			var usings = String.Join("", definition.Usings);
			var targetNamespace = definition.TargetNamespace is null
				? null
				: $"namespace {definition.TargetNamespace};";

			var code = $@"// <auto-generated />
#nullable enable

{usings} 
{targetNamespace}

{declaration}

#nullable disable
";

			return code;
		}
	}
}