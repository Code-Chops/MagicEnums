namespace CodeChops.MagicEnums.UnitTests.Custom;

public class CustomClassEnumTests
{
	[Fact]
	public void CustomClassEnum_HasCorrectValues()
	{
		Assert.Equal(nameof(CustomClassEnumMock.ValueA), CustomClassEnumMock.ValueA.Value.Text);
		Assert.Equal(nameof(CustomClassEnumMock.ValueB), CustomClassEnumMock.ValueB.Value.Text);

		Assert.Equal(nameof(CustomClassEnumMock.ValueA), CustomClassEnumMock.GetSingleMember(nameof(CustomClassEnumMock.ValueA)).Value.Text);
		Assert.Equal(nameof(CustomClassEnumMock.ValueB), CustomClassEnumMock.GetSingleMember(nameof(CustomClassEnumMock.ValueB)).Value.Text);
	}

	[Fact]
	public void CustomClassEnum_NameCount_IsCorrect()
	{
		Assert.Equal(2, CustomClassEnumMock.GetMemberCount());
	}

	[Fact]
	public void CustomClassEnum_ValueCount_IsCorrect()
	{
		Assert.Equal(2, CustomClassEnumMock.GetUniqueValueCount());
	}

	[Fact]
	public void CustomClassEnum_DefaultValue_IsCorrect()
	{
		Assert.Null(CustomClassEnumMock.GetDefaultValue());
	}

	[Fact]
	public void CustomClassEnum_Equals_IsCorrect()
	{
		Assert.True(CustomClassEnumMock.ValueA			== CustomClassEnumMock.ValueA.Value);
		Assert.True(CustomClassEnumMock.ValueA			!= CustomClassEnumMock.ValueB);
		Assert.True(CustomClassEnumMock.ValueA.Value	!= CustomClassEnumMock.ValueB.Value);

		Assert.Equal(CustomClassEnumMock.ValueA,			CustomClassEnumMock.ValueA.Value);
		Assert.NotEqual(CustomClassEnumMock.ValueA,			CustomClassEnumMock.ValueB);
		Assert.NotEqual(CustomClassEnumMock.ValueA.Value,	CustomClassEnumMock.ValueB.Value);
	}
}