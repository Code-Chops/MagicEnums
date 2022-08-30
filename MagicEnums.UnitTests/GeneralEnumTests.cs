﻿namespace CodeChops.MagicEnums.UnitTests;

public record GeneralEnumTests : MagicStringEnum<GeneralEnumTests>
{
	[Fact]
	public void Enum_WithSameNames_ShouldThrow_UsingCreateMember()
	{
		Assert.Throws<ArgumentException>(CreateMembersWithSameName);

		static void CreateMembersWithSameName()
		{
			CreateMember("1", name: nameof(Enum));
			CreateMember(null!, name: nameof(Enum));
		}
	}
	
	[Fact]
	public void Enum_WithSameNames_ShouldNotThrow_UsingGetOrCreateMember()
	{
		GetOrCreateMember("1", name: nameof(Enum));
		GetOrCreateMember(null!, name: nameof(Enum));
	}

	[Fact]
	public void Enum_WithSameValues_ShouldNotThrow()
	{
		// ReSharper disable once ExplicitCallerInfoArgument
		CreateMember("1", name: "Name1");
		// ReSharper disable once ExplicitCallerInfoArgument
		CreateMember(null!, name: "Name2");
	}
}