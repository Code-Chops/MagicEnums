using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.UnitTests.Concurrency;

[DisableConcurrency]
internal record StaticNotConcurrentEnumMock : MagicEnum<StaticNotConcurrentEnumMock>
{
	public static StaticNotConcurrentEnumMock ValueA { get; } = CreateMember();
}

[DisableConcurrency]
internal record DynamicNotConcurrentEnumMock : MagicEnum<DynamicNotConcurrentEnumMock>
{
	public static DynamicNotConcurrentEnumMock ValueA { get; } = CreateMember();

	public static void CreateDynamicTestMember() => CreateMember("DynamicMember");
}