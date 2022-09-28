using System.Diagnostics.CodeAnalysis;

namespace CodeChops.MagicEnums.Core;

public interface IMagicEnumCore<TSelf, TValue> : IMagicEnum<TValue>, IEnumerable<TSelf>
	where TSelf : IMagicEnumCore<TSelf, TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue>
{
	public new static abstract TValue? GetDefaultValue();
	public new static abstract int GetMemberCount();
	public new static abstract int GetUniqueValueCount();
	public new static abstract IEnumerable<TSelf> GetMembers();
	public new static abstract IEnumerable<TValue> GetValues();
	public static abstract bool TryGetSingleMember(string memberName, [NotNullWhen(true)] out TSelf? member);
	public new static abstract TSelf GetSingleMember(string memberName);
	public static abstract bool TryGetSingleMember(TValue memberValue, [NotNullWhen(true)] out TSelf? member);
	public new static abstract TSelf GetSingleMember(TValue memberValue);
	public static abstract bool TryGetMembers(TValue memberValue, [NotNullWhen(true)] out IReadOnlyCollection<TSelf>? members);
	public new static abstract IEnumerable<TSelf> GetMembers(TValue memberValue);
}