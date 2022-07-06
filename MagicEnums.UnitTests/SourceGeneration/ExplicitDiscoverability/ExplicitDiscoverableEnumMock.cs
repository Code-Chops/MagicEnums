using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.ExplicitDiscoverability;

[DiscoverableEnumMembers(implicitDiscoverability: false)]
public partial record ExplicitDiscoverableEnumMock : MagicEnum<ExplicitDiscoverableEnumMock>;