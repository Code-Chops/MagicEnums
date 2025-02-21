namespace CodeChops.MagicEnums.UnitTests;

public record GeneralEnumTests : MagicStringEnum<GeneralEnumTests>
{
    [Fact]
    public void Enum_WithSameNames_ShouldThrow_UsingCreateMember()
    {
        Assert.Throws<InvalidOperationException>(CreateMembersWithSameName);

        static void CreateMembersWithSameName()
        {
            CreateMember("1", name: nameof(Enum));
            CreateMember("2", name: nameof(Enum));
        }
    }
    
    [Fact]
    public void Enum_WithSameNames_ShouldNotThrow_UsingGetOrCreateMember()
    {
        GetOrCreateMember(name: nameof(Enum), "1");
        GetOrCreateMember(name: nameof(Enum), "2");
    }

    [Fact]
    public void Enum_WithSameValues_ShouldNotThrow()
    {
        // ReSharper disable once ExplicitCallerInfoArgument
        CreateMember("1", name: "Name1");
        // ReSharper disable once ExplicitCallerInfoArgument
        CreateMember("1", name: "Name2");
    }
}