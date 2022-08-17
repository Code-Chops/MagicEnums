namespace CodeChops.MagicEnums;

public interface IMagicEnum<TValue> : IMember<TValue>, IComparable<TValue>, IMagicEnum
	where TValue : IEquatable<TValue>, IComparable<TValue>
{
}

public interface IMagicEnum : IId
{
}