namespace CodeChops.MagicEnums.UnitTests.Custom;

public class CustomStructEnumTests
{
	[Fact]
	public void CustomStructEnum_HasCorrectValues()
	{
		Assert.Equal(nameof(CustomStructEnumMock.ValueA), CustomStructEnumMock.ValueA.Value.Text);
		Assert.Equal(nameof(CustomStructEnumMock.ValueB), CustomStructEnumMock.ValueB.Value.Text);

		Assert.Equal(nameof(CustomStructEnumMock.ValueA), CustomStructEnumMock.GetSingleMember(nameof(CustomStructEnumMock.ValueA)).Value.Text);
		Assert.Equal(nameof(CustomStructEnumMock.ValueB), CustomStructEnumMock.GetSingleMember(nameof(CustomStructEnumMock.ValueB)).Value.Text);
	}

	[Fact]
	public void CustomStructEnum_NameCount_IsCorrect()
	{
		Assert.Equal(2, CustomStructEnumMock.GetMemberCount());
	}

	[Fact]
	public void CustomStructEnum_ValueCount_IsCorrect()
	{
		Assert.Equal(2, CustomStructEnumMock.GetUniqueValueCount());
	}

	[Fact]
	public void CustomStructEnum_DefaultValue_IsCorrect()
	{
		Assert.Null(CustomStructEnumMock.GetDefaultValue());
	}

	[Fact]
	public void CustomStructEnum_Equals_IsCorrect()
	{
		Assert.True(CustomStructEnumMock.ValueA			== CustomStructEnumMock.ValueA.Value);
		Assert.True(CustomStructEnumMock.ValueA			!= CustomStructEnumMock.ValueB);
		Assert.True(CustomStructEnumMock.ValueA.Value	!= CustomStructEnumMock.ValueB.Value);

		Assert.Equal(CustomStructEnumMock.ValueA,			CustomStructEnumMock.ValueA.Value);
		Assert.NotEqual(CustomStructEnumMock.ValueA,		CustomStructEnumMock.ValueB);
		Assert.NotEqual(CustomStructEnumMock.ValueA.Value,	CustomStructEnumMock.ValueB.Value);
	}
}