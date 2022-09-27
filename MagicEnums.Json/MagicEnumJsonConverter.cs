using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace CodeChops.MagicEnums.Json;

internal sealed class MagicEnumJsonConverter<TMagicEnum> : JsonConverter<TMagicEnum>
	where TMagicEnum : IMagicEnum
{
	public override bool CanConvert(Type typeToConvert) 
		=> typeToConvert.IsAssignableTo(typeof(IMagicEnum));
	
	private static IMemoryCache GetSingleMemberMethodCache { get; } = new MemoryCache(new MemoryCacheOptions());
	private ImmutableDictionary<string, IMagicEnum> EnumsByName { get; }
	private const char EnumDelimiter = '.';

	public MagicEnumJsonConverter(IEnumerable<IMagicEnum> magicEnums)
	{
		this.EnumsByName = magicEnums.ToImmutableDictionary(GetEnumName);	
	}

	public override TMagicEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// Check for null values
		if (reader.TokenType == JsonTokenType.Null) return default;

		if (reader.TokenType != JsonTokenType.String) throw new JsonException($"Unexpected token found in JSON: {reader.TokenType}. Expected: {JsonTokenType.String}.");
		
		var enumIdentifier = reader.GetString() ?? throw new JsonException($"Unable to retrieve enum identifier when trying to deserialize {typeToConvert.Name}.");
		var delimiterIndex = enumIdentifier.IndexOf(EnumDelimiter);
		if (delimiterIndex == -1) throw new JsonException($"No MagicEnum identifier delimiter ('{EnumDelimiter}') found in {enumIdentifier}.");

		var enumName = enumIdentifier[..delimiterIndex];
		var enumMemberName = enumIdentifier[(delimiterIndex + 1)..];

		if (!GetSingleMemberMethodCache.TryGetValue(enumName, out MethodInfo? getSingleMemberMethod))
		{
			getSingleMemberMethod = RetrieveGetSingleMemberMethod(typeToConvert);

			// Can't find the method on the runtime type, find the injected enum.
			if (getSingleMemberMethod is null)
			{
				if (!this.EnumsByName.TryGetValue(enumName, out var injectedEnum))
					throw new JsonException($"Error while deserializing JSON for {typeToConvert.Name}. Unable to find the concrete MagicEnum. Did you forget to inject enum {enumName}?");

				getSingleMemberMethod = RetrieveGetSingleMemberMethod(injectedEnum.GetType());

				if (getSingleMemberMethod is null)
					throw new JsonException($"Error while deserializing JSON for {typeToConvert.Name}. Unable to find the concrete MagicEnum. Did you inject the wrong enum {enumName}?");
			}
			
			GetSingleMemberMethodCache.Set(enumName, getSingleMemberMethod);
		}
		
		var magicEnum = (TMagicEnum)getSingleMemberMethod!.Invoke(obj: null, parameters: new object?[] { enumMemberName })!;
		return magicEnum;


		static MethodInfo? RetrieveGetSingleMemberMethod(Type type)
		{
			var method = type
				.GetMethods(bindingAttr: BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.SingleOrDefault(method => method.Name == nameof(MagicEnumDummy.GetSingleMember) && method.GetParameters().Any(parameter => parameter.ParameterType == typeof(string)));

			return method;
		}
	}
	
	private static string GetEnumName(IMagicEnum magicEnum)
	{
		var name = magicEnum.GetType().Name;
		var indexOfLessThan = name.IndexOf('`');
		
		return indexOfLessThan == -1
			? name
			: name[..indexOfLessThan];
	}

	public override void Write(Utf8JsonWriter writer, TMagicEnum magicEnum, JsonSerializerOptions options) 
		=> writer.WriteStringValue($"{GetEnumName(magicEnum)}{EnumDelimiter}{magicEnum.Name}");

	// ReSharper disable once ClassNeverInstantiated.Local
	private record MagicEnumDummy : MagicEnum<MagicEnumDummy>;
}