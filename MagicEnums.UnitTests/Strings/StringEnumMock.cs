namespace CodeChops.MagicEnums.UnitTests.Strings;

public partial record StringEnum : MagicStringEnum<StringEnum>
{
	public static StringEnum ValueA { get; } = Create();
	public static StringEnum ValueB { get; } = Create();
	public static StringEnum ValueC { get; } = Create();
	public static StringEnum ValueD { get; } = Create();
}