namespace CodeChops.MagicEnums.Json.UnitTests;

public record MagicEnumMock1 : MagicEnum<MagicEnumMock1>
{
    public static MagicEnumMock1 Value1 { get; } = CreateMember(1);
    public static MagicEnumMock1 Value2 { get; } = CreateMember(7);
}

public record MagicEnumMock2 : MagicEnum<MagicEnumMock2>
{
    public static MagicEnumMock2 Value3 { get; } = CreateMember(0);
    public static MagicEnumMock2 Value4 { get; } = CreateMember(6);
}