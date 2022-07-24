using System.Diagnostics.CodeAnalysis;

namespace CodeChops.MagicEnums.UnitTests.Numbers;

public record NumberEnumConcurrencyTests : MagicEnum<NumberEnumConcurrencyTests>
{
	/// <summary>
	/// Multiple threads should create enum options with implicit incremental values. The order is not guaranteed. 
	/// </summary>
	[Fact]
	[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
	public async Task NumberEnumConcurrency_WithImplicitIncrementalNumber_IsCorrect()
	{
		int index;
		for (index = 0; index < 10000; index += 4)
		{
			var taskA = Task.Run(() => CreateMember(index.ToString()));
			var taskB = Task.Run(() => CreateMember((index + 1).ToString()));
			var taskC = Task.Run(() => CreateMember((index + 2).ToString()));
			var taskD = Task.Run(() => CreateMember((index + 3).ToString()));
			await Task.WhenAll(taskA, taskB, taskC, taskD);
		}
		
		index = 0;
		var options = GetEnumerable().OrderBy(option => Int32.Parse(option.Name));
		foreach (var item in options)
		{
			Assert.Equal(index.ToString(), item.Name);
			index++;
		}
		
		index = 0;
		options = GetEnumerable().OrderBy(option => option.Value);
		foreach (var item in options)
		{
			Assert.Equal(index, item.Value);
			index++;
		}
	}
}