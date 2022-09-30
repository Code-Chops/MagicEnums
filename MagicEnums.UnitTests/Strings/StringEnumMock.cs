namespace CodeChops.MagicEnums.UnitTests.Strings;

internal record StringEnum : MagicStringEnum<StringEnum>
{
	public static StringEnum ValueA { get; } = CreateMember();
	public static StringEnum ValueB { get; } = CreateMember();
	public static StringEnum ValueC { get; } = CreateMember();
	public static StringEnum ValueD { get; } = CreateMember();
}