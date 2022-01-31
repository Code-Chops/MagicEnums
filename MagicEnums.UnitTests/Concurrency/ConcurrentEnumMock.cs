namespace CodeChops.MagicEnums.UnitTests.Concurrency;

internal record StaticConcurrentEnumMock : MagicEnum<StaticConcurrentEnumMock>
{
	public static StaticConcurrentEnumMock ValueA { get; } = CreateMember();
}


internal record DynamicConcurrentEnumMock : MagicEnum<DynamicConcurrentEnumMock>
{
	public static DynamicConcurrentEnumMock ValueA { get; } = CreateMember();

	public static void CreateDynamicTestMember() => CreateMember("DynamicMember");
}