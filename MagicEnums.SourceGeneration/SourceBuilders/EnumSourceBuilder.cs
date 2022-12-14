﻿namespace CodeChops.MagicEnums.SourceGeneration.SourceBuilders;

internal static class EnumSourceBuilder
{
	/// <summary>
	/// Creates a partial record of the enum definition which includes the discovered enum members. It also generates an extension class for the explicit enum definitions.
	/// </summary>
	public static void CreateSource(SourceProductionContext context, ImmutableArray<DiscoveredEnumMember> allDiscoveredMembers, 
		Dictionary<string, EnumDefinition> enumDefinitionsByName, AnalyzerConfigOptionsProvider configOptionsProvider)
	{
		if (enumDefinitionsByName.Count == 0) return;

		// Get the discovered members and their definition.
		// Exclude the members that have no definition, or the members that are discovered while their definition doesn't allow it.
		var relevantDiscoveredMembersByDefinition = allDiscoveredMembers
			.GroupBy(member => enumDefinitionsByName.TryGetValue(member.EnumName, out var definition) ? definition : null)
			.Where(grouping => grouping.Key is not null)
			.ToDictionary(grouping => grouping.Key, grouping => grouping.Where(member => grouping.Key!.DiscoverabilityMode == member.DiscoverabilityMode));

		foreach (var definition in enumDefinitionsByName.Values)
		{
			var relevantDiscoveredMembers = relevantDiscoveredMembersByDefinition.TryGetValue(definition, out var members)
				? members.ToList()
				: new List<DiscoveredEnumMember>();

			var enumCode = CreateEnumSource(definition!, relevantDiscoveredMembers);

			var fileName = $"{definition.Namespace}.{definition.Name}";
			
			context.AddSource(FileNameHelpers.GetFileName(fileName, configOptionsProvider), SourceText.From(enumCode, Encoding.UTF8));
		}
	}

	private static string CreateEnumSource(EnumDefinition definition, List<DiscoveredEnumMember> relevantDiscoveredMembers)
	{
		var code = new StringBuilder();

		// Place the members that are discovered in the enum definition file itself first. The order can be relevant because the value of enum members can be implicitly incremental.
		// Do a distinct on the file path and line position so the members will be deduplicated while typing their invocation.
		// Also do a distinct on the member name.		
		relevantDiscoveredMembers = relevantDiscoveredMembers
			.OrderByDescending(member => member.FilePath == definition.FilePath)
			.GroupBy(member => (member.FilePath, member.LinePosition))
			.Select(group => group.First())
			.GroupBy(member => member.Name)
			.Select(membersByName => membersByName.First())
			.ToList();

		var members = definition.AttributeMembers
			.Concat(relevantDiscoveredMembers)
			.ToList();

		// Is used for correct enum member outlining.
		var longestMemberNameLength = members
			.Select(member => member.Name)
			.OrderByDescending(name => name.Length)
			.FirstOrDefault()?.Length ?? 0;

		// Create the whole source.
		code.AppendLine($@"// <auto-generated />
#nullable enable
");

		code.AppendLine(GetUsings);
		code.AppendLine(GetNamespaceDeclaration);
		code.AppendLine(GetEnum);
		code.AppendLine(GetExtensions, trimEnd: true);
		
		code.AppendLine(@"
#nullable restore");

		return code.ToString();

		
		// Creates a using for the definition of the enum value type (or null if not applicable).
		string? GetUsings()
		{
			var namespaces = new []
			{
				"System",
				"CodeChops.MagicEnums",
				definition.ValueTypeNamespace ?? "System",
			};

			var namespaceUsings = namespaces
				.OrderBy(ns => ns.StartsWith("CodeChops"))
				.ThenBy(ns => ns)
				.Aggregate(new StringBuilder(), (sb, ns) => sb.AppendLine($"using {ns};"))
				.ToString();

			return namespaceUsings;
		}

		
		// Creates the namespace definition of the location of the enum definition (or null if the namespace is not defined).
		string? GetNamespaceDeclaration()
		{
			if (definition.Namespace is null) return null;

			var code = $@"namespace {definition.Namespace};";
			return code;
		}

		
		// Creates the partial enum record (or null if the enum has no members).
		string? GetEnum()
		{
			if (!members.Any()) return null;

			var code = new StringBuilder();

			// Create the comments on the enum record.
			code.Append($@"
/// <summary>
/// <list type=""bullet"">");

			foreach (var member in members)
			{
				var outlineSpaces = new String(' ', longestMemberNameLength - member.Name.Length);

				code.Append($@"
/// <item><c><![CDATA[ {member.Name}{outlineSpaces} = {member.Value ?? "?"} ]]></c></item>");
			}

			code.Append($@"
/// </list>
/// </summary>");

			// Define the enum record.
			code.Append($@"
{definition.AccessModifier} partial record {(definition.IsStruct ? "struct " : "class")} {definition.Name}
{{");

			// Add the discovered members to the enum record.
			foreach (var member in members)
			{
				// Create the comment on the enum member.
				if (member.Value is not null || member.Comment is not null)
				{
					code.Append($@"
	/// <summary>");

					if (member.Comment is not null)
					{
						code.Append($@"
	/// <para>{member.Comment}</para>");
					}

					if (member.Value is not null)
					{
						code.Append($@"
	/// <c><![CDATA[ (value: {member.Value}) ]]></c>");
					}

					code.Append($@"
	/// </summary>");
				}

				// Create the enum member itself.
				var outlineSpaces = new string(' ', longestMemberNameLength - member.Name.Length);
				code.Append(@$"
	public static {definition.Name} {member.Name} {{ get; }} {outlineSpaces}= CreateMember({member.Value});
");
			}

			code.TrimEnd().Append($@"
}}
");

			return code.ToString();
		}


		string GetExtensions() 
			=> @$"
/// <summary>
/// Call this method in order to create discovered enum members while invoking them (on the fly). So enum members are automatically deleted when not being used.
/// </summary>
{definition.AccessModifier.Replace(" sealed", "")} static class {definition.Name}Extensions
{{
	public static {definition.Name} {MagicEnumSourceGenerator.GenerateMethodName}(this {definition.Name} member, {definition.ValueTypeName}? value = null, string? comment = null) 
		=> member;
}}
";
	}
}