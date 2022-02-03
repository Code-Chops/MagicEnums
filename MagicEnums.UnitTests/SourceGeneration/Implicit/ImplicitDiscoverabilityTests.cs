﻿using Xunit;

namespace CodeChops.MagicEnums.UnitTests.SourceGeneration.Implicit;

public class ImplicitDiscoverabilityTests
{
	[Fact]
	public void ImplicitDiscoverability_Members_ShouldBeCreated()
	{
		var a = ImplicitDiscoverableEnumMock.NewMemberA;
		var b = ImplicitDiscoverableEnumMock.NewMemberB.GenerateMember();

		Assert.True(a is not null);
		Assert.True(b is not null);
	}
}