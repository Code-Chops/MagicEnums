namespace CodeChops.MagicEnums.UnitTests.Concurrency;

public record ExplicitValueConcurrencyTests : MagicEnum<ExplicitValueConcurrencyTests>
{
	/// <summary>
	/// Multiple threads should create enum options with explicit incremental values. The order is not guaranteed. 
	/// </summary>
	[Fact]
	[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
	public async Task EnumConcurrency_WithExplicitIncrementalNumber_ShouldBeCorrect()
	{
		int index;
		for (index = 0; index < 10000; index += 4)
		{
			var taskA = Task.Run(() => CreateMember(value: index, enforcedName: index.ToString()));
			var taskB = Task.Run(() => CreateMember(value: index + 1, enforcedName: (index + 1).ToString()));
			var taskC = Task.Run(() => CreateMember(value: index + 2, enforcedName: (index + 2).ToString()));
			var taskD = Task.Run(() => CreateMember(value: index + 3, enforcedName: (index + 3).ToString()));
			await Task.WhenAll(taskA, taskB, taskC, taskD);
		}

		index = 0;
		var members = GetEnumerable().OrderBy(member => member.Value);
		foreach (var member in members)
		{
			Assert.Equal(index, member.Value);
			index++;
		}
	}

	[Fact]
	public void EnumConcurrency_ConcurrencyDisabled_AfterStaticBuildup_ShouldNotBeInConcurrentState()
	{
		Assert.False(StaticNotConcurrentEnumMock.IsInConcurrentState);
	}

	[Fact]
	public void EnumConcurrency_ConcurrencyDisabled_AfterMemberDynamicallyAdded_ShouldNotBeInConcurrentState()
	{
		DynamicNotConcurrentEnumMock.CreateDynamicTestMember();

		Assert.False(DynamicNotConcurrentEnumMock.IsInConcurrentState);
	}

	[Fact]
	public void EnumConcurrency_ConcurrencyEnabled_AfterStaticBuildup_ShouldBeInNotConcurrentState()
	{
		Assert.False(StaticNotConcurrentEnumMock.IsInConcurrentState);
	}

	[Fact]
	public void EnumConcurrency_ConcurrencyEnabled_AfterMemberDynamicallyAdded_ShouldBeInNotConcurrentState()
	{
		DynamicConcurrentEnumMock.CreateDynamicTestMember();

		Assert.True(DynamicConcurrentEnumMock.IsInConcurrentState);
	}
}