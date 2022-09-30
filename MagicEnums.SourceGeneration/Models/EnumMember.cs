namespace CodeChops.MagicEnums.SourceGeneration.Models;

internal record EnumMember
{
	public string Name { get; }
	public object? Value { get; }
	public string? Comment { get; }

	public EnumMember(string name, object? value = null, string? comment = null)
	{
		this.Name = String.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
		this.Value = value;
		this.Comment = String.IsNullOrWhiteSpace(comment) ? null : comment;
	}

	public EnumMember(AttributeData data)
	{
		if (!data.TryGetArguments(out var argumentsByName))
			throw new Exception($"Could not retrieve attribute parameters of attribute {data.AttributeClass?.Name}.");

		this.Name = (string)argumentsByName![nameof(this.Name)].Value!;

		this.Value = argumentsByName.TryGetValue(nameof(this.Value), out var valueArgument) 
			? valueArgument.Type?.GetFullTypeNameWithoutGenericParameters() == "global::System.String" ? $"\"{valueArgument.Value}\"" : valueArgument.Value
			: null;

		this.Comment = (string?)(argumentsByName.TryGetValue(nameof(this.Comment), out var commentArgument) ? commentArgument.Value : null);
	}
}