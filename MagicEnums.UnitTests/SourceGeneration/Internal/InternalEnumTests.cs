namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.Internal;

public class InternalEnumTests
{
	[Fact]
	public void Implicit_Members_ShouldBeCreated()
	{
		var internalEnum = new InternalEnumMock();

		Assert.True(internalEnum is not null);
	}
}