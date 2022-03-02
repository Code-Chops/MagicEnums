using Microsoft.CodeAnalysis;

namespace CodeChops.MagicEnums.SourceGeneration.Extensions;

internal static class AttributeDataExtensions
{
	/// <summary>
	/// Tries to get the arguments of the attribute.
	/// </summary>
	public static bool TryGetArguments(
		this AttributeData attributeData,
		out Dictionary<string, (string Type, object? Value)>? argumentByNames)
	{
		argumentByNames = new Dictionary<string, (string Type, object? Value)>(StringComparer.OrdinalIgnoreCase);
		var constructorParameters = attributeData.AttributeConstructor?.Parameters;

		if (constructorParameters is null) 
		{
			return false; 
		}

		// Start with an indexed list of names for mandatory arguments
		var argumentNames = constructorParameters.Value.Select(x => x.Name).ToList();

		var allArguments = attributeData.ConstructorArguments
			.Select((info, index) => new KeyValuePair<string, TypedConstant>(argumentNames[index], info));

		foreach (var argument in allArguments)
		{
			argumentByNames.Add(argument.Key, (argument.Value.Type!.GetTypeFullName(), argument.Value.Value));
		}

		return true;
	}
}
