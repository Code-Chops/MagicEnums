namespace CodeChops.MagicEnums.UnitTests.Numbers.Flags;

public record FlagEnumTests : MagicFlagsEnum<FlagEnumTests>
{
    private static readonly FlagEnumTests A = CreateMember();
    private static readonly FlagEnumTests B = CreateMember(1 << 0);
    private static readonly FlagEnumTests C = CreateMember(1 << 1);
    private static readonly FlagEnumTests D = CreateMember(1 << 2);
    private static readonly FlagEnumTests E = CreateMember(1 << 3);
    private static readonly FlagEnumTests F = CreateMember();
    private static readonly FlagEnumTests G = CreateMember();
    private static readonly FlagEnumTests H = CreateMember();

    [Fact]
    public void FlagEnum_ValuesAreCorrect()
    {
        Assert.Equal(0,     A.Value);
        Assert.Equal(1,     B.Value);
        Assert.Equal(2,     C.Value);
        Assert.Equal(4,     D.Value);
        Assert.Equal(8,     E.Value);
        Assert.Equal(16,     F.Value);
        Assert.Equal(32,     G.Value);
        Assert.Equal(64,    H.Value);
    }
}