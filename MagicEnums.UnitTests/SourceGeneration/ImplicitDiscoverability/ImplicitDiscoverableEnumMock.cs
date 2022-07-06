using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.ImplicitDiscoverability;

[DiscoverableEnumMembers(implicitDiscoverability: true)]
public partial record ImplicitDiscoverableEnumMock : MagicEnum<ImplicitDiscoverableEnumMock>;