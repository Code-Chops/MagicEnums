using Xunit;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.Explicit;

public class ExplicitDiscoverabilityTests
{
	[Fact]
	public void ExplicitDiscoverability_Members_ShouldBeCreated()
	{
		var a = ExplicitDiscoverableEnumMock.NewMember.CreateMember(1, "Value is one");
		
		Assert.True(a is not null);
	}
}