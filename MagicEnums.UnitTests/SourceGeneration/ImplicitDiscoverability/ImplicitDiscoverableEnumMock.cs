using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.ImplicitDiscoverability;

[DiscoverEnumMembers(implicitly: true)]
public partial record ImplicitDiscoverableEnumMock : MagicEnum<ImplicitDiscoverableEnumMock>;