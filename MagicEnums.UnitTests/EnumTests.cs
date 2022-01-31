using CodeChops.MagicEnums.Core;
using Xunit;

namespace CodeChops.MagicEnums.UnitTests;

public record EnumTests : MagicEnumCore<EnumTests, bool>
{
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
		CreateMember(false, "Name1");
		CreateMember(false, "Name2");
	}
}