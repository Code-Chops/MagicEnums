namespace CodeChops.MagicEnums.UnitTests.Numbers;

public class NumberEnumTests
{
	[Fact]
	public void NumberEnum_InitialImplicitValue_IsZero()
	{
		Assert.Equal(0u, NumberEnumMock.InitialImplicitValue.Value);
	}

	[Fact]
	public void NumberEnum_InitialImplicitValue_IsDefaultValue()
	{
		Assert.True(NumberEnumMock.GetDefaultValue() == NumberEnumMock.InitialImplicitValue.Value);
	}

	[Fact]
	public void NumberEnum_SubsequentImplicitValue_IsIncremental()
	{
		Assert.Equal(1u, NumberEnumMock.ImplicitValue.Value);
		Assert.Equal(8u, NumberEnumMock.SubsequentImplicitValue.Value);
	}

	[Fact]
	public void NumberEnum_ExplicitValue_HasProvidedValue()
	{
		Assert.Equal(6u, NumberEnumMock.NonIncrementalValue.Value);
		Assert.Equal(7u, NumberEnumMock.NonExistingExplicitValue.Value);
		Assert.Equal(7u, NumberEnumMock.ExistingExplicitValue.Value);
	}

	[Fact]
	public void NumberEnum_GetValue_WorksCorrect()
	{
		Assert.Equal(6u, NumberEnumMock.GetSingleMember(nameof(NumberEnumMock.NonIncrementalValue)).Value);
		Assert.Equal(7u, NumberEnumMock.GetSingleMember(nameof(NumberEnumMock.NonExistingExplicitValue)).Value);
		Assert.Equal(7u, NumberEnumMock.GetSingleMember(nameof(NumberEnumMock.ExistingExplicitValue)).Value);
	}

	[Fact]
	public void NumberEnum_GetName_WorksCorrect()
	{
		Assert.Equal(nameof(NumberEnumMock.NonIncrementalValue), NumberEnumMock.GetSingleMember(6u).Name);
	}

	[Fact]
	public void NumberEnum_NameCount_IsCorrect()
	{
		Assert.Equal(8, NumberEnumMock.GetMemberCount());
	}

	[Fact]
	public void NumberEnum_ValueCount_IsCorrect()
	{
		Assert.Equal(7, NumberEnumMock.GetUniqueValueCount());
	}

	[Fact]
	public void NumberEnum_HasCorrectDefaultValue()
	{
		Assert.Equal(0ul, NumberEnumMock.GetDefaultValue());
	}

	[Fact]
	public void NumberEnum_CorrectEquals()
	{
		Assert.True(NumberEnumMock.InitialImplicitValue			== NumberEnumMock.InitialImplicitValue.Value);
		Assert.True(NumberEnumMock.InitialImplicitValue			!= NumberEnumMock.ImplicitValue);
		Assert.True(NumberEnumMock.InitialImplicitValue.Value	!= NumberEnumMock.ImplicitValue.Value);
		Assert.True(NumberEnumMock.NonExistingExplicitValue		== NumberEnumMock.ExistingExplicitValue);

		Assert.Equal(NumberEnumMock.InitialImplicitValue,			NumberEnumMock.InitialImplicitValue.Value);
		Assert.NotEqual(NumberEnumMock.InitialImplicitValue,		NumberEnumMock.ImplicitValue);
		Assert.NotEqual(NumberEnumMock.InitialImplicitValue.Value,	NumberEnumMock.ImplicitValue.Value);
		Assert.Equal(NumberEnumMock.NonExistingExplicitValue.Value, NumberEnumMock.ExistingExplicitValue.Value);
	}
}