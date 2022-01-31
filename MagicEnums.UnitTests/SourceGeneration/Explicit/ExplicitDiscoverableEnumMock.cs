using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.Explicit;

[DiscoverableEnumMembers(implicitDiscoverability: false)]
internal partial record ExplicitDiscoverableEnumMock : MagicEnum<ExplicitDiscoverableEnumMock>
{
}