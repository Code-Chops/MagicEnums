namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.ImplicitDiscoverability;

public class ImplicitTests
{
	[Fact]
	public void Implicit_Members_ShouldBeCreated()
	{
		var a = ImplicitDiscoverableEnumMock.NewMemberA;
		var b = ImplicitDiscoverableEnumMock.NewMemberB.CreateMember();
		var c = ImplicitDiscoverableEnumMock.NewMemberC.CreateMember();
		var d = ImplicitDiscoverableEnumMock.NewMemberD;

		Assert.Equal(0, a);
		Assert.Equal(1, b);
		Assert.Equal(2, c);
		Assert.Equal(3, d);
	}
}