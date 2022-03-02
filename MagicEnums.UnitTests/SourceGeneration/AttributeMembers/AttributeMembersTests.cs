using Xunit;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.AttributeMembers;

public class AttributeMembers
{
	[Fact]
	public void StringAttributeMembers_Members_ShouldBeCreated()
	{
		Assert.Equal("Value1", StringAttributeMembersEnumMock.Name1.Value);
		Assert.Equal("Value2", StringAttributeMembersEnumMock.Name2.Value);
	}

	[Fact]
	public void AttributeMembers_Members_ShouldBeCreated()
	{
		Assert.Equal(1, AttributeMembersEnumMock.Name1.Value);
		Assert.Equal(2, AttributeMembersEnumMock.Name2.Value);
	}
}