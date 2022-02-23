using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.Explicit;

[DiscoverableEnumMembers(implicitDiscoverability: false)]
public partial record ExplicitDiscoverableEnumMock : MagicEnum<ExplicitDiscoverableEnumMock>
{
}