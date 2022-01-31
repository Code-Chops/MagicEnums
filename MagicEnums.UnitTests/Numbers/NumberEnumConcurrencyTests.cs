using Xunit;

namespace CodeChops.MagicEnums.UnitTests.Numbers;

public record NumberEnumConcurrencyTests : MagicEnum<NumberEnumConcurrencyTests, int>
{
	/// <summary>
	/// Multiple threads should create enum options with implicit incremental values. The order is not guaranteed. 
	/// </summary>
	[Fact]
	public async Task NumberEnumConcurrency_WithImplicitIncrementalNumber_IsCorrect()
	{
		var index = 0;
		for (index = 0; index < 100; index += 2)
		{
			var taskA = Task.Run(() => CreateMember(index.ToString()));
			var taskB = Task.Run(() => CreateMember((index + 1).ToString()));
			var taskC = Task.Run(() => GetEnumerable().Select(member => member));
			await Task.WhenAll(taskA, taskB, taskC);
		}

		index = 0;
		var options = GetEnumerable().OrderBy(option => option.Value);
		foreach (var item in options)
		{
			Assert.Equal(index, item.Value);
			index++;
		}
	}
}