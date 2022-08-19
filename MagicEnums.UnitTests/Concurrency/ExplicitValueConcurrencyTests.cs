namespace CodeChops.MagicEnums.UnitTests.Concurrency;

public record ExplicitValueConcurrencyTests : MagicEnum<ExplicitValueConcurrencyTests>
{
	/// <summary>
	/// Multiple threads should create enum options with explicit incremental values using CreateMember. The order is not guaranteed. 
	/// </summary>
	[Fact]
	[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
	public async Task EnumConcurrency_WithExplicitIncrementalNumber_UsingCreateMember_ShouldBeCorrect()
	{
		int index;
		for (index = 0; index < 10000; index += 4)
		{
			var taskA = Task.Run(() => CreateMember(value: index,		name: index.ToString()));
			var taskB = Task.Run(() => CreateMember(value: index + 1, 	name: (index + 1).ToString()));
			var taskC = Task.Run(() => CreateMember(value: index + 2, 	name: (index + 2).ToString()));
			var taskD = Task.Run(() => CreateMember(value: index + 3, 	name: (index + 3).ToString()));
			await Task.WhenAll(taskA, taskB, taskC, taskD);
		}

		index = 0;
		foreach (var value in this.OrderBy(value => value))
		{
			Assert.Equal(index, value);
			index++;
		}
	}

	/// <summary>
	/// Multiple threads should create enum options with explicit incremental values using GetOrCreateMember. The order is not guaranteed. 
	/// </summary>
	[Fact]
	[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
	public async Task EnumConcurrency_WithExplicitIncrementalNumber_UsingGetOrCreateMember_ShouldBeCorrect()
	{
		int index;
		for (index = 0; index < 10000; index += 4)
		{
			var taskA = Task.Run(() => GetOrCreateMember(value: index,		name: index.ToString()));
			var taskB = Task.Run(() => GetOrCreateMember(value: index + 1, 	name: (index + 1).ToString()));
			var taskC = Task.Run(() => GetOrCreateMember(value: index + 2, 	name: (index + 2).ToString()));
			var taskD = Task.Run(() => GetOrCreateMember(value: index + 3, 	name: (index + 3).ToString()));

			var taskE = Task.Run(() => GetOrCreateMember(value: index + 1, 	name: (index + 1).ToString()));
			var taskF = Task.Run(() => GetOrCreateMember(value: index + 2, 	name: (index + 2).ToString()));
			var taskG = Task.Run(() => GetOrCreateMember(value: index + 3, 	name: (index + 3).ToString()));

			await Task.WhenAll(taskA, taskB, taskC, taskD, taskE, taskF, taskG);
		}

		index = 0;
		foreach (var value in this.OrderBy(value => value))
		{
			Assert.Equal(index, value);
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