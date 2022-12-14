namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.ExplicitDiscoverability;

public class ExplicitDiscoverabilityTests
{
	[Fact]
	public void ExplicitDiscoverability_Members_ShouldBeCreated()
	{
		var a = ExplicitDiscoverableEnumMock.NewMember1.CreateMember(1, "Value is one");
		var b = ExplicitDiscoverableEnumMock.NewMember2.CreateMember(comment: "Value is two");
		
		Assert.True(a is not null);
		Assert.Equal(2, b);
	}
}