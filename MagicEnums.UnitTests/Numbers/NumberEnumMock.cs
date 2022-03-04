namespace CodeChops.MagicEnums.UnitTests.Numbers;

internal partial record NumberEnumMock : MagicEnum<NumberEnumMock, ulong>
{
	/// <summary>
	/// An initial implicit value. Should be 0.
	/// </summary>
	public static NumberEnumMock InitialImplicitValue { get; }		= CreateMember();

	/// <summary>
	/// An implicit value preceded by an implicit value <seealso cref="InitialImplicitValue"/>. Should be 1.
	/// </summary>
	public static NumberEnumMock ImplicitValue { get; }				= CreateMember();

	/// <summary>
	/// An explicit non-existing value with a value-gap compared to the previous implicit value <seealso cref="ImplicitValue"/>. Should be 6.
	/// </summary>
	public static NumberEnumMock GappedValue { get; }				= CreateMember(6);

	/// <summary>
	/// An explicit non-existing value. Should be 7.
	/// </summary>
	public static NumberEnumMock NonExistingExplicitValue { get; }	= CreateMember(7);

	/// <summary>
	/// An explicit already-existing value <seealso cref="NonExistingExplicitValue"/>. Should be 7.
	/// </summary>
	public static NumberEnumMock ExistingExplicitValue { get; }		= CreateMember(7);

	/// <summary>
	/// A subsequent implicit value preceded by an explicit value <seealso cref="ExistingExplicitValue"/>. Should be 8.
	/// </summary>
	public static NumberEnumMock SubsequentImplicitValue { get; }	= CreateMember();

	/// <summary>
	/// A value that has the same name (ignoring casing) as <seealso cref="CaseSensitivity"/>. This shouldn't throw. Should be 9.
	/// </summary>
	public static NumberEnumMock Casesensitivity { get; }			= CreateMember();

	/// <summary>
	/// A value that has the same name (ignoring casing) as <seealso cref="Casesensitivity"/>. This shouldn't throw. Should be 10.
	/// </summary>
	public static NumberEnumMock CaseSensitivity { get; }			= CreateMember();
}