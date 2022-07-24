using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.Core;

/// <summary>
/// A magic enum is a flexible and expandable enum which supports other member values than integral types. Inheritance and extensibility are also supported.
/// </summary>
/// <typeparam name="TEnum">The type of the enum itself. Is also the type of each member.</typeparam>
/// <typeparam name="TValue">The type of the enum member value.</typeparam>
internal interface IMagicEnumCore<TEnum, out TValue> : IMember<TValue>, IMagicEnum
	where TEnum : IMagicEnumCore<TEnum, TValue>
	where TValue : notnull
{
	/// <summary>
	/// The default value of the enum.
	/// </summary>
	public static TValue? GetDefaultValue() => default;

	/// <summary>
	/// Get the member count.
	/// </summary>
	public static int GetMemberCount() => MemberByNames.Keys.Count;

	/// <summary>
	/// Get the unique member value count.
	/// </summary>
	public static int GetUniqueValueCount() => MembersByValues.Keys.Count;

	/// <summary>
	/// Enumerates over the members.
	/// </summary>
	public static IEnumerable<TEnum> GetEnumerable() => MemberByNames.Values;

	/// <summary>
	/// Enumerates over the values.
	/// </summary>
	public static IEnumerable<TValue> GetValues() => GetEnumerable().Select(member => member.Value);
	
	/// <summary>
	/// Is true if the dictionary is in a concurrent state.
	/// </summary>
	protected static bool IsInConcurrentState => MemberByNames is ConcurrentDictionary<string, TEnum>;

	/// <summary>
	/// Is true of the enum is in a static creation. The enum does not have to be concurrent during this period.
	/// </summary>
	// ReSharper disable once MemberInitializerValueIgnored
	private static bool IsInStaticBuildup { get; } = true;

	/// <summary>
	/// The concurrency mode of the enum. <see cref="Core.ConcurrencyMode"/>
	/// </summary>
	private static ConcurrencyMode ConcurrencyMode { get; }

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
			.ToDictionary(member => member.Key.Value, member => member.AsEnumerable());

	/// <summary>
	/// A mapping of a member value to one or more members. Don't change this value. Only reset it (to null).
	/// </summary>
	private static IDictionary<TValue, IEnumerable<TEnum>>? _membersByValues;

	/// <summary>
	/// A lock for the dictionary. 
	/// This lock is used for switching between a concurrent and not-concurrent state (when using <see cref="Core.ConcurrencyMode.AdaptiveConcurrency"/>).
	/// </summary>
	private static readonly object DictionaryLock = new();

	static IMagicEnumCore()
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
	protected static TValue GetLastInsertedValue()
	{
		return MembersByValues.LastOrDefault().Key;
	}

	/// <summary>
	/// Creates a new enum member.
	/// </summary>
	/// <param name="memberCreator">A function that creates the member.</param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentNullException">When no name or value has been provided.</exception>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected static TEnum CreateMember(Func<TEnum> memberCreator)
	{
		var memberByNames = MemberByNames;

		// Switch to a concurrent mode if necessary.
		if (ConcurrencyMode == ConcurrencyMode.AdaptiveConcurrency && !IsInConcurrentState && !IsInStaticBuildup)
		{
			return SwitchToConcurrentModeAndAddMember(memberCreator, memberByNames);
		}

		// Create the new member.
		var member = CreateAndAddMemberToDictionary(memberCreator, memberByNames);
		return member;
		

		static TEnum SwitchToConcurrentModeAndAddMember(Func<TEnum> memberCreator, IDictionary<string, TEnum> memberByNames)
		{
			lock (DictionaryLock)
			{
				// Check if we didn't win the race.
				if (MemberByNames != memberByNames)
				{
					var member = CreateAndAddMemberToDictionary(memberCreator, MemberByNames);
					return member;
				}

				// Convert to a concurrent dictionary.
				var concurrentMemberByNames = new ConcurrentDictionary<string, TEnum>(memberByNames);
				var newMember = CreateAndAddMemberToDictionary(memberCreator, concurrentMemberByNames);
				MemberByNames = concurrentMemberByNames;
				return newMember;
			}
		}


		static TEnum CreateAndAddMemberToDictionary(Func<TEnum> memberCreator, IDictionary<string, TEnum> memberByNames)
		{
			var member = memberCreator();
			
			// Adds a new member by name.
			if (!memberByNames.TryAdd(member.Name, member)) throw new ArgumentException($"Name '{member.Name}' already defined in enum {typeof(TEnum).Name} (value: '{member.Value}').");
			_membersByValues = null!;

			return member;
		}
	}

	/// <summary>
	/// Tries to retrieve a single member that has the provided name. 
	/// </summary>
	/// <param name="memberName">The name of the member to retrieve.</param>
	/// <param name="member">The queried member.</param>
	/// <returns>True if a member has been found.</returns>
	/// <exception cref="ArgumentNullException">When name is null.</exception>
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
	public static bool TryGetSingleMember(TValue memberValue, [NotNullWhen(true)] out TEnum? member)
	{
		if (memberValue is null) throw new ArgumentNullException(nameof(memberValue));

		if (!MembersByValues.TryGetValue(memberValue, out var members))
		{
			member = default;
			return false;
		}

		var membersList = members.ToList();
		if (membersList.Count > 1) throw new InvalidOperationException($"Expected enum {typeof(TEnum).Name} to have exactly one member with value '{memberValue}'.");

		member = membersList.First();
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
	public static bool TryGetMembers(TValue memberValue, out IEnumerable<TEnum> members)
		=> memberValue is null
			 ? throw new ArgumentNullException(nameof(memberValue))
			 : MembersByValues.TryGetValue(memberValue, out members!);

	/// <summary>
	/// Returns the member(s) that have the provided value. Throws when no member has been found.
	/// Use <see cref="GetSingleMember(TValue)"/> to retrieve a single member.
	/// </summary>
	/// <param name="memberValue">The value of the member(s) to retrieve.</param>
	/// <returns>The queried member(s).</returns>
	/// <exception cref="ArgumentNullException">When value is null.</exception>
	/// <exception cref="KeyNotFoundException">When no member has been found.</exception>
	public static IEnumerable<TEnum> GetMembers(TValue memberValue)
		=> TryGetMembers(memberValue, out var memberName)
			? memberName
			: throw new KeyNotFoundException($"Unable to find a member with value '{memberValue}' in enum {typeof(TEnum).Name}.");
}