namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.AttributeMembers;

public class AttributeMembers
{
	[Fact]
	public void StringAttributeMembers_Members_ShouldHaveCorrectValue()
	{
		Assert.Equal("Value1", StringAttributeMembersEnumMock.Name1.Value);
		Assert.Equal("Value2", StringAttributeMembersEnumMock.Name2.Value);
		Assert.Equal("Name3", StringAttributeMembersEnumMock.Name3.Value);
	}

	[Fact]
	public void AttributeMembers_Members_ShouldHaveCorrectValue()
	{
		Assert.Equal(1, AttributeMembersEnumMock.Name1.Value);
		Assert.Equal(2, AttributeMembersEnumMock.Name2.Value);
		Assert.Equal(3, AttributeMembersEnumMock.Name3.Value);
	}
}