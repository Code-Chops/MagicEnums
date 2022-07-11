namespace CodeChops.MagicEnums.UnitTests;

public record EnumTests : MagicCustomEnum<EnumTests, bool>
{
	public static EnumTests Member { get; } = CreateMember(false);

	[Fact]
	public void Enum_WithSameNames_ShouldThrow()
	{
		Assert.Throws<ArgumentException>(CreateOptionsWithSameName);

		static void CreateOptionsWithSameName()
		{
			CreateMember(false, nameof(Enum));
			CreateMember(true, nameof(Enum));
		}
	}

	[Fact]
	public void Enum_WithSameValues_ShouldNotThrow()
	{
		// ReSharper disable once ExplicitCallerInfoArgument
		CreateMember(false, "Name1");
		// ReSharper disable once ExplicitCallerInfoArgument
		CreateMember(false, "Name2");
	}

	[Fact]
	public void Enum_ToString_ShouldReturnName()
	{
		Assert.Equal(nameof(Member), Member.ToString());
	}
}