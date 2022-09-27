using System.Text.Json;
using CodeChops.DomainDrivenDesign.DomainModeling.Identities.Serialization.Json;
using CodeChops.DomainDrivenDesign.DomainModeling.Serialization;
using CodeChops.Geometry.Space.Directions.Strict;
using CodeChops.Geometry.Space.Directions.Strict.Definitions;
using Xunit;

namespace CodeChops.MagicEnums.Json.UnitTests;

public class MagicEnumsConversionTests
{
	private const string Json = @$"""{nameof(MagicEnumMock1)}.{nameof(MagicEnumMock1.Value2)}""";
	private const string WrapperJson = @$"{{""{nameof(MagicEnumWrapperContractMock.Enum)}"":""{nameof(MagicEnumMock2)}.{nameof(MagicEnumMock2.Value3)}"",""Direction"":""DiagonalDirection.SouthEast""}}";

	static MagicEnumsConversionTests()
	{
		var magicEnums = new IMagicEnum[] { new MagicEnumMock1(), new MagicEnumMock2() }.Concat(StrictDirectionEnum<int>.GetValues().Select(value => value.UninitializedInstance));

		JsonSerialization.DefaultOptions.Converters.Add(new MagicEnumJsonConverterFactory(magicEnums));
		JsonSerialization.DefaultOptions.Converters.Add(new IdentityJsonConverterFactory());
	}

	[Fact]
	public void Conversion_ToDomainModel_IsCorrect()
	{
		var magicEnum = JsonSerializer.Deserialize<MagicEnumMock1>(Json, JsonSerialization.DefaultOptions)!;
		Assert.NotNull(magicEnum);
		
		Assert.Equal(typeof(MagicEnumMock1), magicEnum.GetType());
		Assert.Equal(MagicEnumMock1.Value2.Name, magicEnum.Name);
		Assert.Equal(MagicEnumMock1.Value2.Value.ToString(), magicEnum.Value.ToString());
	}
	
	[Fact]
	public void Deserialization_MagicEnum_IsCorrect()
	{
		var contract = JsonSerializer.Deserialize<IMagicEnum>(Json, JsonSerialization.DefaultOptions)!;
		Assert.NotNull(contract);

		Assert.Equal(typeof(MagicEnumMock1), contract.GetType());
		Assert.Equal(MagicEnumMock1.Value2.Name, contract.Name);
	}

	[Fact]
	public void Serialization_MagicEnum_IsCorrect()
	{
		var json = JsonSerializer.Serialize(MagicEnumMock1.Value2, JsonSerialization.DefaultOptions);
        
		Assert.Equal(Json, json);
	}

	[Fact]
	public void Serialization_MagicEnum_WithWrapper_ShouldWork()
	{
		var wrapper = new MagicEnumWrapperContractMock(@enum: MagicEnumMock2.Value3, direction: DiagonalDirection<int>.SouthEast);
		var json = JsonSerializer.Serialize(wrapper, JsonSerialization.DefaultOptions);
		
		Assert.Equal(WrapperJson, json);
	}
	
	[Fact]
	public void Deserialization_MagicEnum_WithWrapper_ShouldWork()
	{
		var magicEnum = JsonSerializer.Deserialize<MagicEnumWrapperContractMock>(WrapperJson, JsonSerialization.DefaultOptions)!;
		Assert.NotNull(magicEnum);
		
		Assert.Equal(typeof(MagicEnumWrapperContractMock), magicEnum.GetType());
		Assert.Equal(MagicEnumMock2.Value3.Name, magicEnum.Enum.Name);
	}
}