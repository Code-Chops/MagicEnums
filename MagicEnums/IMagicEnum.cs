namespace CodeChops.MagicEnums;

public interface IMagicEnum<TValue> : IMember<TValue>, IMagicEnum
	where TValue : IEquatable<TValue>, IComparable<TValue>
{
	TValue? GetDefaultValue();
	int GetMemberCount();
	int GetUniqueValueCount();
	IEnumerable<IMagicEnum<TValue>> GetMembers();
	IEnumerable<TValue> GetValues();
	bool TryGetSingleMember(string memberName, [NotNullWhen(true)] out IMagicEnum<TValue>? member);
	IMagicEnum<TValue> GetSingleMember(string memberName);
	bool TryGetSingleMember(TValue memberValue, [NotNullWhen(true)] out IMagicEnum<TValue>? member);
	IMagicEnum<TValue> GetSingleMember(TValue memberValue);
	bool TryGetMembers(TValue memberValue, [NotNullWhen(true)] out IReadOnlyCollection<IMagicEnum<TValue>>? members);
	IEnumerable<IMagicEnum<TValue>> GetMembers(TValue memberValue);
}

public interface IMagicEnum : IMember, IId
{
}