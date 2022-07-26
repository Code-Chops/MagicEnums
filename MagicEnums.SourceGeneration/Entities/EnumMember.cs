namespace CodeChops.MagicEnums.SourceGeneration.Entities;

public record EnumMember : IEnumEntity
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
		if (!data.TryGetArguments(out var arguments))
		{
			throw new Exception($"Could not retrieve attribute parameters of attribute {data.AttributeClass?.Name}.");
		}

		this.Name = (string)arguments![nameof(this.Name)].Value!;

		this.Value = arguments.TryGetValue(nameof(this.Value), out var valueArgument) 
			? valueArgument.Type?.GetFullTypeNameWithGenericParameters() == "System.String" ? $"\"{valueArgument.Value}\"" : valueArgument.Value
			: null;

		this.Comment = (string?)(arguments.TryGetValue(nameof(this.Comment), out var commentArgument) ? commentArgument.Value : null);
	}
}