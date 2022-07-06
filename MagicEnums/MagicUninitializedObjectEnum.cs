using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with an uninitialized object as member value.
/// Use <see cref="MagicStringEnum{TEnum}.CreateMember(string?)"/> to create a member.
/// </summary>
/// <typeparam name="TEnum">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicUninitializedObjectEnum<TEnum> : MagicUninitializedObjectEnum<TEnum, object>
	where TEnum : MagicUninitializedObjectEnum<TEnum>;

/// <summary>
/// An enum with an uninitialized object (of TBaseType) as member value.
/// Use <see cref="MagicStringEnum{TEnum}.CreateMember(string?)"/> to create a member.
/// </summary>
/// <typeparam name="TEnum">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TBaseType">The base type of the enum value.</typeparam>
public abstract record MagicUninitializedObjectEnum<TEnum, TBaseType> : MagicCustomEnum<TEnum, TBaseType>
	where TEnum : MagicUninitializedObjectEnum<TEnum, TBaseType>
	where TBaseType : notnull
{
	protected static TEnum CreateMember<TValue>([CallerMemberName] string name = null!)
	{
		var value = (TBaseType)FormatterServices.GetUninitializedObject(typeof(TValue));
		var id = CreateMember(value, name);

		return id;
	}

	protected static TEnum CreateMember(Type value, [CallerMemberName] string name = null!)
	{
		if (value is null) throw new ArgumentNullException(nameof(value), "A type enum cannot contain a null type.");

		var id = CreateMember(value: (TBaseType)FormatterServices.GetUninitializedObject(value), name);

		return id;
	}
}