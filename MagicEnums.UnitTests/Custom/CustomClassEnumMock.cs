namespace CodeChops.MagicEnums.UnitTests.Custom;

internal partial record CustomClassEnumMock : MagicCustomEnum<CustomClassEnumMock, DataClass>
{
	public static CustomClassEnumMock ValueA { get; } = CreateMember(new DataClass(Text: nameof(ValueA)));
	public static CustomClassEnumMock ValueB { get; } = CreateMember(new DataClass(Text: nameof(ValueB)));
}

internal record DataClass(string Text);