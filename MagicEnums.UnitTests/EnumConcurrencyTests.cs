using System.Linq;
using Xunit;

namespace CodeChops.MagicEnums.UnitTests;

public record EnumConcurrencyTests : MagicEnum<EnumConcurrencyTests, int>
{
	/// <summary>
	/// Multiple threads should create enum options with implicit incremental values. The order is not guaranteed. 
	/// </summary>
	[Fact]
	public async Task EnumConcurrency_WithImplicitIncrementalNumber_ShouldBeCorrect()
	{
		var index = 0;
		for (index = 0; index < 100; index += 2)
		{
			var taskA = Task.Run(() => Create(index, enforcedName: index.ToString()));
			var taskB = Task.Run(() => Create(index + 1, enforcedName: (index + 1).ToString()));
			var taskC = Task.Run(() => GetEnumerable().Select(member => member));
			await Task.WhenAll(taskA, taskB, taskC);
		}

		index = 0;
		var members = GetEnumerable().OrderBy(member => member.Value);
		foreach (var member in members)
		{
			Assert.Equal(index, member.Value);
			index++;
		}
	}
}