namespace CodeChops.MagicEnums.Core;

/// <summary>
/// A magic enum is a flexible and expandable enum which supports other member values than integral types. Inheritance and extensibility are also supported.
/// </summary>
/// <typeparam name="TValue">The type of the enum member value.</typeparam>
internal interface IMagicEnum<TValue>
{
	string? Name { get; }
	TValue? Value { get; }
}