using System.Diagnostics.CodeAnalysis;

namespace CodeChops.MagicEnums.Core;

public interface IMagicEnumCore<TSelf, TValue> : IMagicEnum<TValue>, IEnumerable<TSelf>
	where TSelf : MagicEnumCore<TSelf, TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue>
{
	public static abstract bool TryGetSingleMember(string memberName, [NotNullWhen(true)] out TSelf? member);
	public static abstract TSelf GetSingleMember(string memberName);
	public static abstract bool TryGetSingleMember(TValue memberValue, [NotNullWhen(true)] out TSelf? member);
	public static abstract TSelf GetSingleMember(TValue memberValue);
	public static abstract bool TryGetMembers(TValue memberValue, [NotNullWhen(true)] out IReadOnlyCollection<TSelf>? members);
	public static abstract IEnumerable<TSelf> GetMembers(TValue memberValue);
}