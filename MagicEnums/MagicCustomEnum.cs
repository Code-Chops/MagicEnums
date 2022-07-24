using CodeChops.MagicEnums.Core;

namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with a custom type as member value.
/// Use <see cref="CodeChops.MagicEnums.Core.MagicEnumCore{TEnum, TValue}.CreateMember(TValue, string)"/> to create a member.
/// </summary>
/// <typeparam name="TEnum">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TValue">The type of the value of the enum.</typeparam>
public abstract partial record MagicCustomEnum<TEnum, TValue> : MagicEnumCore<TEnum, TValue>
	where TEnum : MagicCustomEnum<TEnum, TValue>
	where TValue : notnull;