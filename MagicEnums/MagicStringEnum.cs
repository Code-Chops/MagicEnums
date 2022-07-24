using System.Runtime.CompilerServices;
using CodeChops.MagicEnums.Core;

namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with a string as member value.
/// Use <see cref="MagicStringEnum{TEnum}.CreateMember(string?)"/> to create a member.
/// </summary>
/// <typeparam name="TEnum">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract partial record MagicStringEnum<TEnum> : MagicEnumCore<TEnum, string>
	where TEnum : MagicStringEnum<TEnum>;