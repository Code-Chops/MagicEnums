﻿using CodeChops.MagicEnums.Core;

namespace CodeChops.MagicEnums.UnitTests.Custom;

internal partial record CustomStructEnumMock : MagicEnumCore<CustomStructEnumMock, DataStruct>
{
	public static CustomStructEnumMock ValueA { get; } = CreateMember(new DataStruct(Text: nameof(ValueA)));
	public static CustomStructEnumMock ValueB { get; } = CreateMember(new DataStruct(Text: nameof(ValueB)));
}

internal record DataStruct(string Text);