using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.Core;

[DisableConcurrency]
internal sealed record ConcurrencyMode : MagicEnum<ConcurrencyMode>
{
	/// <summary>
	/// Default:
	/// The enum will intelligently switch to a concurrent and non-concurrent state:
	/// - The enum will be created in a concurrent state.
	/// - When the enum instance has been constructed, it will fall back to a non-current state (to optimize speed and memory usage).
	/// - If a new member is added to the enum after construction, it will switch back to a concurrent state and stays in this state.
	/// </summary>
	public static ConcurrencyMode AdaptiveConcurrency { get; }	= CreateMember();

	/// <summary>
	/// The enum is not concurrent throughout its lifetime.
	/// Warning! Only use this label when the members are created from a static context.
	/// </summary>
	public static ConcurrencyMode NeverConcurrent { get; }		= CreateMember();
}