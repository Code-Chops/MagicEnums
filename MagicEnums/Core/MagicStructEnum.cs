using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using CodeChops.MagicEnums.Core.Members;

namespace CodeChops.MagicEnums.Core;

/// <summary>
/// This a test to easily create a struct enum.
/// </summary>
internal record struct MagicStructEnum : IMagicEnumCore<MagicStructEnum, int>
{
	/// <summary>
	/// Returns the name of the enum member.
	/// </summary>
	public override string? ToString() => this.Name;

	public bool Equals(MagicStructEnum? other) => other is not null && this.Value.Equals(other.Value);
	public override int GetHashCode() => this.Value.GetHashCode();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator int?(MagicStructEnum magicEnum) => magicEnum.Value;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator MagicStructEnum(int value) => GetSingleMember(value);

	/// <inheritdoc cref="IMember{TValue}.Name"/>
	public string Name { get; internal init; } = default!;

	/// <inheritdoc cref="IMember{TValue}.Value"/>
	public int Value { get; internal init; } = default!;

	public int Index { get; internal init; }

	public MagicStructEnum(string name, int value, int index)
	{
		this.Name = name ?? throw new ArgumentNullException(nameof(name));
		this.Value = value;
		this.Index = index;
	}

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetDefaultValue"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int? GetDefaultValue() => IMagicEnumCore<MagicStructEnum, int>.GetDefaultValue();

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetMemberCount"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetMemberCount() => IMagicEnumCore<MagicStructEnum, int>.GetMemberCount();

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetUniqueValueCount"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetUniqueValueCount() => IMagicEnumCore<MagicStructEnum, int>.GetUniqueValueCount();

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetEnumerable()"/>
	public static IEnumerable<MagicStructEnum> GetEnumerable() => IMagicEnumCore<MagicStructEnum, int>.GetEnumerable();

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetEnumerable()"/>
	public static MagicStructEnum CreateMember(int value, string name) 
		=> IMagicEnumCore<MagicStructEnum, int>.CreateMember(value, name, () => CachedUnitializedMember with { Name = name, Value = value });

	/// <summary>
	/// Used to create new members.
	/// </summary>
	private static readonly MagicStructEnum CachedUnitializedMember = (MagicStructEnum)FormatterServices.GetUninitializedObject(typeof(MagicStructEnum));

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.TryGetSingleMember(string, out TEnum)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetSingleMember(string memberName, [NotNullWhen(true)] out MagicStructEnum member) => IMagicEnumCore<MagicStructEnum, int>.TryGetSingleMember(memberName, out member);

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetSingleMember(string)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static MagicStructEnum GetSingleMember(string memberName) => IMagicEnumCore<MagicStructEnum, int>.GetSingleMember(memberName);

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.TryGetSingleMember(TValue, out TEnum)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetSingleMember(int memberValue, [NotNullWhen(true)] out MagicStructEnum member) => IMagicEnumCore<MagicStructEnum, int>.TryGetSingleMember(memberValue, out member);

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetSingleMember(TValue)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static MagicStructEnum GetSingleMember(int memberValue) => IMagicEnumCore<MagicStructEnum, int>.GetSingleMember(memberValue);

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetSingleMember(TValue)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetMembers(int memberValue, [NotNullWhen(true)] out IEnumerable<MagicStructEnum> members) => IMagicEnumCore<MagicStructEnum, int>.TryGetMembers(memberValue, out members);

	/// <inheritdoc cref="IMagicEnumCore{TEnum, TValue}.GetMembers(TValue)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<MagicStructEnum> GetMembers(int memberValue) => IMagicEnumCore<MagicStructEnum, int>.GetMembers(memberValue);
}