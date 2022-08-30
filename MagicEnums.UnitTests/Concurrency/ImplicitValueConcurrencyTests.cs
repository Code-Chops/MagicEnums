namespace CodeChops.MagicEnums.UnitTests.Concurrency;

// ReSharper disable AccessToModifiedClosure
public record ImplicitValueConcurrencyTests : MagicEnum<ImplicitValueConcurrencyTests, ulong>
{
	/// <summary>
	/// Multiple threads should create enum options with implicit incremental values. The order is not guaranteed. 
	/// </summary>
	[Fact]
	public async Task EnumConcurrency_WithImplicitIncrementalNumber_IsCorrect()
	{
		ulong index;
		for (index = 0; index < 10000; index += 4)
		{
			var taskA = Task.Run(() => CreateMember(name: index.ToString()));
			var taskB = Task.Run(() => CreateMember(name: (index + 1).ToString()));
			var taskC = Task.Run(() => CreateMember(name: (index + 2).ToString()));
			var taskD = Task.Run(() => CreateMember(name: (index + 3).ToString()));
			await Task.WhenAll(taskA, taskB, taskC, taskD);
		}

		index = 0;
		var valuesOrderedByName = this.Select(value => UInt64.Parse(value.Name)).OrderBy(value => value);
		foreach (var value in valuesOrderedByName)
		{
			Assert.Equal(index, value);
			index++;
		}
		
		index = 0;
		var orderedValues = this.OrderBy(value => value).ToList();
		foreach (var value in orderedValues)
		{
			Assert.Equal(index, value.Value);
			index++;
		}
	}
}