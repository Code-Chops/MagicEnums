using CodeChops.DomainDrivenDesign.DomainModeling.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace CodeChops.MagicEnums.Json;

public static class RegistrationExtensions
{
	public static IServiceCollection AddMagicEnumJsonSerialization(this IServiceCollection services, IEnumerable<IMagicEnum> magicEnums)
	{
		JsonSerialization.DefaultOptions.Converters.Add(new MagicEnumJsonConverterFactory(magicEnums));

		return services;
	}
}