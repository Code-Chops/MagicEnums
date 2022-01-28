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
			Create(false, nameof(Enum));
			Create(true, nameof(Enum));
		}
	}

	[Fact]
	public void Enum_WithSameValues_ShouldNotThrow()
	{
		Create(false, "Name1");
		Create(false, "Name2");
	}
}