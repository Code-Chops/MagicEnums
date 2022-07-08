﻿using System.Runtime.CompilerServices;
using CodeChops.MagicEnums.Core;

namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with a string as member value.
/// Use <see cref="MagicStringEnum{TEnum}.CreateMember(string?)"/> to create a member.
/// </summary>
/// <typeparam name="TEnum">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract partial record MagicStringEnum<TEnum> : MagicEnumCore<TEnum, string>
	where TEnum : MagicStringEnum<TEnum>
{
	/// <summary>
	/// Creates a new enum member with the member name as string value.
	/// </summary>
	/// <param name="enforcedName">
	/// The name of the new member.
	/// Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten. 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	public static TEnum CreateMember([CallerMemberName] string? enforcedName = null) => CreateMember(enforcedName!, enforcedName!);
}