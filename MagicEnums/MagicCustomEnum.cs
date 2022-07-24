using System.Runtime.CompilerServices;
using CodeChops.MagicEnums.Core;

namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with a custom type as member value.
/// Use <see cref="CodeChops.MagicEnums.MagicCustomEnum{TEnum, TValue}.CreateMember(TValue, string)"/> to create a member.
/// </summary>
/// <typeparam name="TEnum">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TValue">The type of the value of the enum.</typeparam>
public abstract partial record MagicCustomEnum<TEnum, TValue> : MagicEnumCore<TEnum, TValue>
	where TEnum : MagicCustomEnum<TEnum, TValue>
	where TValue : notnull
{
	/// <summary>
	/// Creates a new enum member with value TValue.
	/// </summary>
	/// <param name="value">The value of the new member. Inserting null values is not supported.</param>
	/// <param name="enforcedName">
	/// The name of the new member.
	/// Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten. 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected new static TEnum CreateMember(TValue value, [CallerMemberName] string enforcedName = null!) => MagicEnumCore<TEnum, TValue>.CreateMember(value, enforcedName);
}