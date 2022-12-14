using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.ExplicitDiscoverability;

[DiscoverEnumMembers(implicitly: false)]
public partial record ExplicitDiscoverableEnumMock : MagicEnum<ExplicitDiscoverableEnumMock>;