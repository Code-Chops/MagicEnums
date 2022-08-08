﻿using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.ExplicitDiscoverability;

[DiscoverEnumMembers(implicitDiscoverability: false)]
public partial record ExplicitDiscoverableEnumMock : MagicEnum<ExplicitDiscoverableEnumMock>;