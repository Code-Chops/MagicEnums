using Xunit;

namespace CodeChops.MagicEnums.UnitTests.InitializedObjects;

public class UninitializedObjectEnumTest
{
	[Fact]
	public void ObjectEnum_ProvidedValue_IsCorrect()
	{
		Assert.Equal(typeof(Cat), UninitializedObjectEnumMock.Cat.Value?.GetType());
		Assert.Equal(typeof(Dog), UninitializedObjectEnumMock.Dog.Value?.GetType());

		Assert.Equal(typeof(Cat), UninitializedObjectWithBaseTypeEnumMock.Cat.Value?.GetType());
		Assert.Equal(typeof(Dog), UninitializedObjectWithBaseTypeEnumMock.Dog.Value?.GetType());
	}

	[Fact]
	public void ObjectEnum_DefaultValue_IsNull()
	{
		Assert.Null(UninitializedObjectEnumMock.GetDefaultValue());
	}

	[Fact]
	public void ObjectEnum_Equals_IsCorrect()
	{
		Assert.True(UninitializedObjectEnumMock.Cat			== UninitializedObjectEnumMock.Cat);
		Assert.True(UninitializedObjectEnumMock.Cat.Value	== UninitializedObjectEnumMock.Cat.Value);
		Assert.True(UninitializedObjectEnumMock.Dog			== UninitializedObjectEnumMock.Dog);
		Assert.True(UninitializedObjectEnumMock.Dog.Value	== UninitializedObjectEnumMock.Dog.Value);
	
		Assert.True(UninitializedObjectWithBaseTypeEnumMock.Cat			== UninitializedObjectWithBaseTypeEnumMock.Cat);
		Assert.True(UninitializedObjectWithBaseTypeEnumMock.Cat.Value	== UninitializedObjectWithBaseTypeEnumMock.Cat.Value);
		Assert.True(UninitializedObjectWithBaseTypeEnumMock.Dog			== UninitializedObjectWithBaseTypeEnumMock.Dog);
		Assert.True(UninitializedObjectWithBaseTypeEnumMock.Dog.Value	== UninitializedObjectWithBaseTypeEnumMock.Dog.Value);

		Assert.Equal(UninitializedObjectEnumMock.Cat,		UninitializedObjectEnumMock.Cat);
		Assert.Equal(UninitializedObjectEnumMock.Cat.Value,	UninitializedObjectEnumMock.Cat.Value);
		Assert.Equal(UninitializedObjectEnumMock.Dog,		UninitializedObjectEnumMock.Dog);
		Assert.Equal(UninitializedObjectEnumMock.Dog.Value,	UninitializedObjectEnumMock.Dog.Value);

		Assert.Equal(UninitializedObjectWithBaseTypeEnumMock.Cat,		UninitializedObjectWithBaseTypeEnumMock.Cat);
		Assert.Equal(UninitializedObjectWithBaseTypeEnumMock.Cat.Value,	UninitializedObjectWithBaseTypeEnumMock.Cat.Value);
		Assert.Equal(UninitializedObjectWithBaseTypeEnumMock.Dog,		UninitializedObjectWithBaseTypeEnumMock.Dog);
		Assert.Equal(UninitializedObjectWithBaseTypeEnumMock.Dog.Value,	UninitializedObjectWithBaseTypeEnumMock.Dog.Value);
	}
}