namespace CodeChops.MagicEnums.Attributes;

/// <summary>
/// Add this attribute to the enum to disable concurrency and therefore optimise the memory usage and speed.
/// Warning! Only use this label when you are sure that no race conditions can take place.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class DisableConcurrencyAttribute : Attribute
{
}