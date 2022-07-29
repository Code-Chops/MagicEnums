using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with an uninitialized object as member value.
/// Use <see cref="MagicStringEnum{TSelf}.CreateMember(string?)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicUninitializedObjectEnum<TSelf> : MagicUninitializedObjectEnum<TSelf, object>
	where TSelf : MagicUninitializedObjectEnum<TSelf>;

/// <summary>
/// An enum with an uninitialized object (of TBaseType) as member value.
/// Use <see cref="MagicStringEnum{TSelf}.CreateMember(string?)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TBaseType">The base type of the enum value.</typeparam>
public abstract record MagicUninitializedObjectEnum<TSelf, TBaseType> : MagicCustomEnum<TSelf, TBaseType>
	where TSelf : MagicUninitializedObjectEnum<TSelf, TBaseType>
	where TBaseType : notnull
{
	protected static TSelf CreateMember<TValue>([CallerMemberName] string name = null!)
	{
		var value = (TBaseType)FormatterServices.GetUninitializedObject(typeof(TValue));
		var id = CreateMember(value, name);

		return id;
	}

	protected static TSelf CreateMember(Type value, [CallerMemberName] string name = null!)
	{
		if (value is null) throw new ArgumentNullException(nameof(value), "A type enum cannot contain a null type.");

		var id = CreateMember(value: (TBaseType)FormatterServices.GetUninitializedObject(value), name);

		return id;
	}
}