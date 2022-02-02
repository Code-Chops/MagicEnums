using Xunit;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.Implicit;

public class ImplicitDiscoverabilityTests
{
	[Fact]
	public void ImplicitDiscoverability_Members_ShouldBeCreated()
	{
		var a = ImplicitDiscoverableEnumMock.NewMemberA;
		var b = ImplicitDiscoverableEnumMock.NewMemberB.GenerateMember();
		var c = ImplicitDiscoverableEnumMock.Nee.GenerateMember(6, "Je bent een hondje"); // Dit werkt nog niet en los regelvervanging op. En enums met dezelfde naam

		Assert.True(a is not null);
		Assert.True(b is not null);
	}
}