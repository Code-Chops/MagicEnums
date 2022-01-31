using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.Implicit;

[DiscoverableEnumMembers(implicitDiscoverability: true)]
internal partial record ImplicitDiscoverableEnumMock : MagicEnum<ImplicitDiscoverableEnumMock>
{
}