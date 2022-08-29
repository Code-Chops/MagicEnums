namespace CodeChops.MagicEnums.UnitTests.Concurrency;

public record ImplicitFlagsValueConcurrencyTests : MagicFlagsEnum<ImplicitFlagsValueConcurrencyTests, ulong>
{
	/// <summary>
	/// Multiple threads should create enum options with implicit incremental bit-shifting values. The order is not guaranteed. 
	/// </summary>
	[Fact]
	public async Task EnumFlagsConcurrency_WithImplicitIncrementalNumber_IsCorrect()
	{
		int index;
		for (index = 0; index < 16; index += 4)
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
			Assert.Equal(index, (int)value);
			index++;
		}
		
		ulong bitshift = 0;
		index = 0;
		var binarySortedValues = this.OrderBy(value => Convert.ToString((int)(ulong)value, 2));
		foreach (var value in binarySortedValues)
		{
			Assert.Equal(bitshift, value.Value);
			bitshift = index == 0 ? 1 : (ulong)1 << index;
			index++;
		}
	}
}