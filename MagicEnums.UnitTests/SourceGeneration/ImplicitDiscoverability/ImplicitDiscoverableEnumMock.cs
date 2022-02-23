using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.Implicit;

[DiscoverableEnumMembers(implicitDiscoverability: true)]
public partial record ImplicitDiscoverableEnumMock : MagicEnum<ImplicitDiscoverableEnumMock>
{
}