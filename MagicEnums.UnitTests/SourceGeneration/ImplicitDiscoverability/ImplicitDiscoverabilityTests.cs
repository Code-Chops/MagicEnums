namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.ImplicitDiscoverability;

public class ImplicitDiscoverabilityTests
{
	[Fact]
	public void ImplicitDiscoverability_Members_ShouldBeCreated()
	{
		var a = ImplicitDiscoverableEnumMock.NewMemberA;
		var b = ImplicitDiscoverableEnumMock.NewMemberB.CreateMember();

		Assert.True(a is not null);
		Assert.True(b is not null);
	}
}