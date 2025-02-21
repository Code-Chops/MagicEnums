using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeChops.MagicEnums.Json;

public sealed class MagicEnumJsonConverterFactory(IEnumerable<IMagicEnum>? magicEnums = null) : JsonConverterFactory
{
    private List<IMagicEnum> MagicEnums { get; } = magicEnums?.ToList() ?? [];

    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsAssignableTo(typeof(IMagicEnum));

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converter = (JsonConverter)Activator.CreateInstance(
            type: typeof(MagicEnumJsonConverter<>).MakeGenericType(typeToConvert),
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: [this.MagicEnums],
            culture: null)!;

        return converter;
    }
}