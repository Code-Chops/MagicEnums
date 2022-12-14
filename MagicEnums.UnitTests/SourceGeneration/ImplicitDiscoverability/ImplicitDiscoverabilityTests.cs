namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.ImplicitDiscoverability;

public class ImplicitTests
{
	[Fact]
	public void Implicit_Members_ShouldBeCreated()
	{
		var a = ImplicitDiscoverableEnumMock.NewMemberA;
		var b = ImplicitDiscoverableEnumMock.NewMemberB.CreateMember();

		Assert.Equal(0, a);
		Assert.Equal(1, b);
	}
}