namespace CodeChops.MagicEnums;

public interface IMagicEnum<out TValue> : IMember<TValue>, IMagicEnum
	where TValue : IEquatable<TValue>, IComparable<TValue>
{
}

public interface IMagicEnum : IMember, IId
{
}