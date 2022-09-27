using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeChops.MagicEnums.Json;

public sealed class MagicEnumJsonConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) 
		=> typeToConvert.IsAssignableTo(typeof(IMagicEnum));

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var converter = (JsonConverter)Activator.CreateInstance(
			type: typeof(MagicEnumJsonConverter<>).MakeGenericType(typeToConvert),
			bindingAttr: BindingFlags.Instance | BindingFlags.Public,
			binder: null,
			args: new object[] { this.MagicEnums },
			culture: null)!;

		return converter;
	}
	
	private List<IMagicEnum> MagicEnums { get; }

	public MagicEnumJsonConverterFactory(IEnumerable<IMagicEnum>? magicEnums = null)
	{
		this.MagicEnums = magicEnums?.ToList() ?? new List<IMagicEnum>();
	}
}