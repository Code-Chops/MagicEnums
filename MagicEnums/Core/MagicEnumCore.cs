using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using CodeChops.Identities;

namespace CodeChops.MagicEnums.Core;

/// <summary>
/// A magic enum is a flexible and expandable enum which supports other member values than integral types. Inheritance and extensibility are also supported.
/// </summary>
/// <typeparam name="TEnum">The type of the enum itself. Is also the type of each member.</typeparam>
/// <typeparam name="TValue">The type of the value of the enum.</typeparam>
public abstract partial record MagicEnumCore<TEnum, TValue> : IMagicEnumCore<TEnum, TValue>, IId<TValue>
	where TEnum : MagicEnumCore<TEnum, TValue>
	where TValue : notnull
{
	/// <summary>
	/// Returns the name of the enum member.
	/// </summary>
	public sealed override string ToString() => this.Name;

	public virtual bool Equals(MagicEnumCore<TEnum, TValue>? other) 
		=> other is not null && this.Value.Equals(other.Value);
	public override int GetHashCode() => this.Value.GetHashCode();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator TValue(MagicEnumCore<TEnum, TValue> magicEnum) => magicEnum.Value;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator MagicEnumCore<TEnum, TValue>(TValue value) => GetSingleMember(value);

	/// <inheritdoc cref="IMember{TValue}.Name"/>
	public string Name { get; private init; } = default!;

	/// <inheritdoc cref="IMember{TValue}.Value"/>
	public TValue Value { get; private init; } = default!;

	public object GetValue() => this.Value;

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetDefaultValue"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TValue? GetDefaultValue() => IMagicEnumCore<TEnum, TValue>.GetDefaultValue();

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetMemberCount"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetMemberCount() => IMagicEnumCore<TEnum, TValue>.GetMemberCount();

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetUniqueValueCount"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetUniqueValueCount() => IMagicEnumCore<TEnum, TValue>.GetUniqueValueCount();

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetEnumerable"/>
	public static IEnumerable<TEnum> GetEnumerable() => IMagicEnumCore<TEnum, TValue>.GetEnumerable();

	protected static TEnum CreateMember(TValue value, string name)
		=> IMagicEnumCore<TEnum, TValue>.CreateMember(value, name, () => CachedUninitializedMember with { Name = name, Value = value });

	/// <summary>
	/// Used to create new members.
	/// </summary>
	private static readonly TEnum CachedUninitializedMember = (TEnum)FormatterServices.GetUninitializedObject(typeof(TEnum));

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.TryGetSingleMember(string, out TEnum)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetSingleMember(string memberName, [NotNullWhen(true)] out TEnum? member) => IMagicEnumCore<TEnum, TValue>.TryGetSingleMember(memberName, out member);

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetSingleMember(string)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TEnum GetSingleMember(string memberName) => IMagicEnumCore<TEnum, TValue>.GetSingleMember(memberName);

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.TryGetSingleMember(TValue, out TEnum)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetSingleMember(TValue memberValue, [NotNullWhen(true)] out TEnum? member) => IMagicEnumCore<TEnum, TValue>.TryGetSingleMember(memberValue, out member);

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetSingleMember(TValue)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TEnum GetSingleMember(TValue memberValue) => IMagicEnumCore<TEnum, TValue>.GetSingleMember(memberValue);

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetSingleMember(TValue)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetMembers(TValue memberValue, [NotNullWhen(true)] out IEnumerable<TEnum>? members) => IMagicEnumCore<TEnum, TValue>.TryGetMembers(memberValue, out members);

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetMembers(TValue)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<TEnum> GetMembers(TValue memberValue) => IMagicEnumCore<TEnum, TValue>.GetMembers(memberValue);
}