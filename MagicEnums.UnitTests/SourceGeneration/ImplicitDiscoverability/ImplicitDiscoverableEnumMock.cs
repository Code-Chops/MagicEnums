using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.ImplicitDiscoverability;

[DiscoverEnumMembers(implicitDiscoverability: true)]
public partial record ImplicitDiscoverableEnumMock : MagicEnum<ImplicitDiscoverableEnumMock>;