using System.Text.Json;
using CodeChops.DomainModeling.Serialization.Json;
using CodeChops.Geometry.Space.Directions.Strict;
using CodeChops.Geometry.Space.Directions.Strict.Definitions;
using Xunit;

namespace CodeChops.MagicEnums.Json.UnitTests;

public class MagicEnumsConversionTests
{
	private const string Json = @$"""{nameof(MagicEnumMock1)}.{nameof(MagicEnumMock1.Value2)}""";
	private const string WrapperJson = @$"{{""{nameof(MagicEnumWrapperContractMock.Enum)}"":""{nameof(MagicEnumMock2)}.{nameof(MagicEnumMock2.Value3)}"",""Direction"":""DiagonalDirection.SouthEast""}}";

	private JsonSerializerOptions JsonSerializerOptions { get; }

	public MagicEnumsConversionTests()
	{
		var magicEnums = new IMagicEnum[] { new MagicEnumMock1(), new MagicEnumMock2() }.Concat(StrictDirectionEnum<int>.GetInstances());

		this.JsonSerializerOptions = new()
		{
			WriteIndented = false,
			Converters = { new MagicEnumJsonConverterFactory(magicEnums), new ValueObjectJsonConverterFactory() }
		};
	}

	[Fact]
	public void Conversion_ToDomainModel_IsCorrect()
	{
		var magicEnum = JsonSerializer.Deserialize<MagicEnumMock1>(Json, this.JsonSerializerOptions)!;
		Assert.NotNull(magicEnum);

		Assert.Equal(typeof(MagicEnumMock1), magicEnum.GetType());
		Assert.Equal(MagicEnumMock1.Value2.Name, magicEnum.Name);
		Assert.Equal(MagicEnumMock1.Value2.Value.ToString(), magicEnum.Value.ToString());
	}

	[Fact]
	public void Deserialization_MagicEnum_IsCorrect()
	{
		var contract = JsonSerializer.Deserialize<IMagicEnum>(Json, this.JsonSerializerOptions)!;
		Assert.NotNull(contract);

		Assert.Equal(typeof(MagicEnumMock1), contract.GetType());
		Assert.Equal(MagicEnumMock1.Value2.Name, contract.Name);
	}

	[Fact]
	public void Serialization_MagicEnum_IsCorrect()
	{
		var json = JsonSerializer.Serialize(MagicEnumMock1.Value2, this.JsonSerializerOptions);

		Assert.Equal(Json, json);
	}

	[Fact]
	public void Serialization_MagicEnum_WithWrapper_ShouldWork()
	{
		var wrapper = new MagicEnumWrapperContractMock(@enum: MagicEnumMock2.Value3, direction: DiagonalDirection<int>.SouthEast);
		var json = JsonSerializer.Serialize(wrapper, this.JsonSerializerOptions);

		Assert.Equal(WrapperJson, json);
	}

	[Fact]
	public void Deserialization_MagicEnum_WithWrapper_ShouldWork()
	{
		var magicEnum = JsonSerializer.Deserialize<MagicEnumWrapperContractMock>(WrapperJson, this.JsonSerializerOptions)!;
		Assert.NotNull(magicEnum);

		Assert.Equal(typeof(MagicEnumWrapperContractMock), magicEnum.GetType());
		Assert.Equal(MagicEnumMock2.Value3.Name, magicEnum.Enum.Name);
	}
}