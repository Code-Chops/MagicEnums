using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.Core;

/// <summary>
/// A magic enum is a flexible and expandable enum which supports other member values than integral types. Inheritance and extensibility are also supported.
/// </summary>
/// <typeparam name="TEnum">The type of the enum itself. Is also the type of each member.</typeparam>
/// <typeparam name="TValue">The type of the value of the enum.</typeparam>
[CloneAsInternal("CodeChops.MagicEnums.Core.Internal")]
public abstract partial record MagicEnumCore<TEnum, TValue> : IMagicEnumCore<TValue>
	where TEnum : MagicEnumCore<TEnum, TValue>
{
	/// <summary>
	/// Returns the name of the enum member.
	/// </summary>
	public sealed override string? ToString() => this.Name;

	public virtual bool Equals(MagicEnumCore<TEnum, TValue>? other)
		=> ReferenceEquals(this, other) || other is not null && (this.Value is null ? other.Value is null : this.Value.Equals(other.Value));
	public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator TValue?(MagicEnumCore<TEnum, TValue> magicEnum) => magicEnum.Value;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator MagicEnumCore<TEnum, TValue>(TValue value) => GetSingleMember(value);

	/// <summary>
	/// The name of the member.
	/// </summary>
	public string Name { get; private init; } = default!;

	/// <summary>
	/// The value of the member.
	/// </summary>
	public TValue? Value { get; protected init; }

	/// <summary>
	/// The default value of the enum.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TValue? GetDefaultValue() => default;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetMemberCount() => MemberByNames.Keys.Count;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetUniqueValueCount() => MembersByValues.Keys.Count;

	/// <summary>
	/// Enumerates over the members.
	/// </summary>
	public static IEnumerable<TEnum> GetEnumerable() => MemberByNames.Values;

	/// <summary>
	/// Is true if the dictionary is in a concurrent state.
	/// </summary>
	protected static bool IsInConcurrentState => MemberByNames is ConcurrentDictionary<string, TEnum>;

	/// <summary>
	/// Is true of the enum is in a static creation. The enum does not have to be concurrent during this period.
	/// </summary>
	private static bool IsInStaticBuildup { get; set; } = true;

	/// <summary>
	/// The concurrency mode of the enum. <see cref="Core.ConcurrencyMode"/>
	/// </summary>
	private static ConcurrencyMode ConcurrencyMode { get; set; }

	/// <summary>
	/// A mapping of a member name to a single member.
	/// </summary>
	private static IDictionary<string, TEnum> MemberByNames { get; set; } = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// A mapping of a member value to one or more members.
	/// </summary>
	private static IDictionary<TValue, IEnumerable<TEnum>> MembersByValues 
		=> _membersByValues ??= MemberByNames
			.GroupBy(memberByName => memberByName.Value, memberByName => memberByName.Value)
			.ToDictionary(member => member.Key.Value!, member => member.AsEnumerable());

	/// <summary>
	/// A mapping of a member value to one or more members. Don't change this value. Only reset it (to null).
	/// </summary>
	private static IDictionary<TValue, IEnumerable<TEnum>> _membersByValues = null!;

	/// <summary>
	/// Used to create new members.
	/// </summary>
	private static readonly TEnum CachedUnitializedMember = (TEnum)FormatterServices.GetUninitializedObject(typeof(TEnum));

	/// <summary>
	/// A lock for the dictionary. 
	/// This lock is used for switching between a concurrent and not-concurrent state (when using <see cref="ConcurrencyMode.AdaptiveConcurrency"/>).
	/// </summary>
	private static readonly object DictionaryLock = new();

	static MagicEnumCore()
	{
		ConcurrencyMode = typeof(TEnum).GetCustomAttributes(typeof(DisableConcurrencyAttribute), inherit: true).Any()
			? ConcurrencyMode.NeverConcurrent
			: ConcurrencyMode.AdaptiveConcurrency;

		// Forces to run the static constructor of the user-defined enum, so the Create method is called for every member (in code line order).
		RuntimeHelpers.RunClassConstructor(typeof(TEnum).TypeHandle);

		IsInStaticBuildup = false;
	}

	/// <summary>
	/// Gets the last inserted value of the enum.
	/// </summary>
	protected static TValue? GetLastInsertedValue()
	{
		return MembersByValues.LastOrDefault().Key;
	}

	/// <summary>
	/// Creates a new enum member.
	/// </summary>
	/// <param name="newValue">The value of the new member. Inserting null values is not supported.</param>
	/// <param name="enforcedName">
	/// The name of the new member.
	/// Don't provide this parameter, so the property name of the enum will automaticaly be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten. 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentNullException">When no name or value has been provided.</exception>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected static TEnum CreateMember(TValue newValue, [CallerMemberName] string? enforcedName = null)
	{
		if (String.IsNullOrWhiteSpace(enforcedName)) throw new ArgumentNullException(nameof(enforcedName));
		if (newValue is null) throw new ArgumentNullException(nameof(newValue));

		// Create the new member.
		var newMember = CachedUnitializedMember with
		{
			Name = enforcedName,
			Value = newValue,
		};

		var memberByNames = MemberByNames;

		// Switch to a concurrent mode if necessary.
		if (ConcurrencyMode == ConcurrencyMode.AdaptiveConcurrency && !IsInConcurrentState && !IsInStaticBuildup)
		{
			SwitchToConcurrentAndAddMember();
			return newMember;
		}

		AddMemberToDictionary(newMember, memberByNames);
		return newMember;
		

		void SwitchToConcurrentAndAddMember()
		{
			lock (DictionaryLock)
			{
				// Check if we won the race.
				if (MemberByNames != memberByNames)
				{
					AddMemberToDictionary(newMember, MemberByNames);
					return;
				}

				// Convert to a concurrent dictionary.
				var concurrentMemberByNames = new ConcurrentDictionary<string, TEnum>(memberByNames);
				AddMemberToDictionary(newMember, concurrentMemberByNames);
				MemberByNames = concurrentMemberByNames;
			}
		}


		static void AddMemberToDictionary(TEnum newMember, IDictionary<string, TEnum> memberByNames)
		{
			// Adds a new member by name.
			if (!memberByNames.TryAdd(newMember.Name, newMember)) throw new ArgumentException($"Name '{newMember.Name}' already defined in enum {typeof(TEnum).Name} (value: '{newMember.Value}').");
			_membersByValues = null!;
		}
	}

	/// <summary>
	/// Tries to retrieve a single member that has the provided name. 
	/// </summary>
	/// <param name="memberName">The name of the member to retrieve.</param>
	/// <param name="member">The queried member.</param>
	/// <returns>True if a member has been found.</returns>
	/// <exception cref="ArgumentNullException">When name is null.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetSingleMember(string memberName, [NotNullWhen(true)] out TEnum? member)
		=> memberName is null
			? throw new ArgumentNullException(memberName)
			: MemberByNames.TryGetValue(memberName, out member);

	/// <summary>
	/// Returns a single member that has the provided name.
	/// </summary>
	/// <param name="memberName">The name of the member to retrieve.</param>
	/// <returns>The queried member.</returns>
	/// <exception cref="ArgumentNullException">When name is null.</exception>
	/// <exception cref="KeyNotFoundException">When no member has been found.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TEnum GetSingleMember(string memberName)
		=> TryGetSingleMember(memberName, out var member)
			? member
			: throw new KeyNotFoundException($"Unable to find a member with name '{memberName}' in enum {typeof(TEnum).Name}.");

	/// <summary>
	/// Tries to return the single member that has the provided value. 
	/// Warning! This method will throw when multiple members have this value.
	/// Use <see cref="TryGetMembers"/> to retrieve multiple members.
	/// </summary>
	/// <param name="memberValue">The value of the member to retrieve.</param>
	/// <param name="member">The queried member.</param>
	/// <returns>True if a single member has been found.</returns>
	/// <exception cref="ArgumentNullException">When value is null.</exception>
	/// <exception cref="InvalidOperationException">When multiple members with the same value have been found.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetSingleMember(TValue memberValue, [NotNullWhen(true)] out TEnum? member)
	{
		if (memberValue is null) throw new ArgumentNullException(nameof(memberValue));

		if (!MembersByValues.TryGetValue(memberValue, out var members))
		{
			member = default;
			return false;
		}

		if (members.Count() > 1) throw new InvalidOperationException($"Expected enum {typeof(TEnum).Name} to have exactly one member with value '{memberValue}'.");

		member = members.First();
		return true;
	}

	/// <summary>
	/// Returns the single member that has the provided value. Throws when no member has been found.
	/// Warning! Multiple members can have the same value. This method will throw when multiple members have this value.
	/// Use <see cref="GetMembers"/> to retrieve multiple members.
	/// </summary>
	/// <param name="memberValue">The value of the member to retrieve.</param>
	/// <returns>The queried member.</returns>
	/// <exception cref="ArgumentNullException">When value is null.</exception>
	/// <exception cref="InvalidOperationException">When multiple members with the same value have been found.</exception>
	/// <exception cref="KeyNotFoundException">When no member has been found.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TEnum GetSingleMember(TValue memberValue)
		=> TryGetSingleMember(memberValue, out var memberName)
			? memberName
			: throw new KeyNotFoundException($"Unable to find a member with value '{memberValue}' in enum {typeof(TEnum).Name}.");

	/// <summary>
	/// Tries to return the member(s) that have the provided value. 
	/// Use <see cref="TryGetSingleMember(TValue, out TEnum)"/> to retrieve a single member.
	/// </summary>
	/// <param name="memberValue">The value of the member(s) to retrieve.</param>
	/// <param name="members">The queried member(s).</param>
	/// <returns>True if one or more members have been found.</returns>
	/// <exception cref="ArgumentNullException">When value is null.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetMembers(TValue memberValue, [NotNullWhen(true)] out IEnumerable<TEnum>? members)
		=> memberValue is null
			 ? throw new ArgumentNullException(nameof(memberValue))
			 : MembersByValues.TryGetValue(memberValue, out members);

	/// <summary>
	/// Returns the member(s) that have the provided value. Throws when no member has been found.
	/// Use <see cref="GetSingleMember(TValue)"/> to retrieve a single member.
	/// </summary>
	/// <param name="memberValue">The value of the member(s) to retrieve.</param>
	/// <returns>The queried member(s).</returns>
	/// <exception cref="ArgumentNullException">When value is null.</exception>
	/// <exception cref="KeyNotFoundException">When no member has been found.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<TEnum> GetMembers(TValue memberValue)
		=> TryGetMembers(memberValue, out var memberName)
			? memberName
			: throw new KeyNotFoundException($"Unable to find a member with value '{memberValue}' in enum {typeof(TEnum).Name}.");
}