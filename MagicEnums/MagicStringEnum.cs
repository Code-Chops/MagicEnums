﻿namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with a string as member value (with ordinal comparison)
/// Use <see cref="MagicStringEnum{TSelf}.CreateMember(string?)"/> to create a member.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicStringEnum<TSelf> : MagicEnumCore<TSelf, string>, IComparable<MagicStringEnum<TSelf>>
	where TSelf : MagicStringEnum<TSelf>
{
	#region Comparison

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(MagicStringEnum<TSelf>? other)
	{
		if (other is null) throw new ArgumentNullException(nameof(other));
		return String.Compare(this.Value, other.Value, StringComparison.Ordinal);
	}

	#endregion
	
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
	public static TSelf CreateMember([CallerMemberName] string? enforcedName = null) => CreateMember(enforcedName!, enforcedName!);
}