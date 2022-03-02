using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.AttributeMembers;

[EnumMember("Name1", "Value1", "Comment1")]
[EnumMember("Name2", "Value2")]
[EnumMember("Name3")]
public partial record StringAttributeMembersEnumMock : MagicStringEnum<StringAttributeMembersEnumMock>
{
}

[EnumMember("Name1", 1, "Comment1")]
[EnumMember("Name2", 2)]
[EnumMember("Name3")]
public partial record AttributeMembersEnumMock : MagicEnum<AttributeMembersEnumMock>
{
}