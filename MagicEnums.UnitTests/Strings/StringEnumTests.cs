using Xunit;

namespace CodeChops.MagicEnums.UnitTests.Strings;

public class StringEnumTests
{
	[Fact]
	public void StringEnum_WithProvidedValue_HasCorrectValues()
	{
		Assert.Equal("ValueA", StringEnum.ValueA.Value);
		Assert.Equal("ValueB", StringEnum.ValueB.Value);
		Assert.Equal("ValueC", StringEnum.ValueC.Value);
		Assert.Equal("ValueD", StringEnum.ValueD.Value);

		Assert.Equal("ValueA", StringEnum.GetSingleMember(nameof(StringEnum.ValueA)).Value);
		Assert.Equal("ValueB", StringEnum.GetSingleMember(nameof(StringEnum.ValueB)).Value);
		Assert.Equal("ValueC", StringEnum.GetSingleMember(nameof(StringEnum.ValueC)).Value);
		Assert.Equal("ValueD", StringEnum.GetSingleMember(nameof(StringEnum.ValueD)).Value);
	}

	[Fact]
	public void StringEnum_NameCount_IsCorrect()
	{
		Assert.Equal(4, StringEnum.GetMemberCount());
	}

	[Fact]
	public void StringEnum_ValueCount_IsCorrect()
	{
		Assert.Equal(4, StringEnum.GetUniqueValueCount());
	}

	[Fact]
	public void StringEnum_DefaultValue_IsCorrect()
	{
		Assert.Null(StringEnum.GetDefaultValue());
	}

	[Fact]
	public void StringEnum_Equals_IsCorrect()
	{
		Assert.True(StringEnum.ValueA		== StringEnum.ValueA);
		Assert.True(StringEnum.ValueA.Value == StringEnum.ValueA.Value);
		Assert.True(StringEnum.ValueA		!= StringEnum.ValueB);
		Assert.True(StringEnum.ValueA.Value != StringEnum.ValueB.Value);

		Assert.Equal(StringEnum.ValueA,				StringEnum.ValueA);
		Assert.Equal(StringEnum.ValueA.Value,		StringEnum.ValueA.Value);
		Assert.NotEqual(StringEnum.ValueA,			StringEnum.ValueB);
		Assert.NotEqual(StringEnum.ValueA.Value,	StringEnum.ValueB.Value);
	}
}