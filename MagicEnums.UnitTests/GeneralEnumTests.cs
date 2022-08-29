namespace CodeChops.MagicEnums.UnitTests;

public record GeneralEnumTests : MagicCustomEnum<GeneralEnumTests, string>
{
	[Fact]
	public void Enum_WithSameNames_ShouldThrow_UsingCreateMember()
	{
		Assert.Throws<ArgumentException>(CreateMembersWithSameName);

		static void CreateMembersWithSameName()
		{
			CreateMember("1", nameof(Enum));
			CreateMember(null!, nameof(Enum));
		}
	}
	
	[Fact]
	public void Enum_WithSameNames_ShouldNotThrow_UsingGetOrCreateMember()
	{
		GetOrCreateMember("1", nameof(Enum));
		GetOrCreateMember(null!, nameof(Enum));
	}

	[Fact]
	public void Enum_WithSameValues_ShouldNotThrow()
	{
		// ReSharper disable once ExplicitCallerInfoArgument
		CreateMember("1", "Name1");
		// ReSharper disable once ExplicitCallerInfoArgument
		CreateMember(null!, "Name2");
	}
}