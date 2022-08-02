using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.Core;

/// <summary>
/// A magic enum is a flexible and expandable enum which supports other member values than integral types. Inheritance and extensibility are also supported.
/// </summary>
/// <typeparam name="TSelf">The type of the enum itself. Is also the type of each member.</typeparam>
/// <typeparam name="TValue">The type of the enum member value.</typeparam>
public abstract record MagicEnumCore<TSelf, TValue> : IMagicEnum
	where TSelf : MagicEnumCore<TSelf, TValue>
	where TValue : notnull
{
	/// <summary>
	/// Returns the name of the enum member.
	/// </summary>
	public sealed override string ToString() => this.Name;
	
	/// <inheritdoc cref="IMember{TValue}.Name"/>
	public string Name { get; private init; } = default!;

	/// <inheritdoc cref="IMember{TValue}.Value"/>
	public TValue Value { get; private init; } = default!;

	#region Comparison
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual bool Equals(MagicEnumCore<TSelf, TValue>? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;
		return this.Value.Equals(other.Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
		=> this.Value.GetHashCode();
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator TValue(MagicEnumCore<TSelf, TValue> magicEnum) => magicEnum.Value;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator MagicEnumCore<TSelf, TValue>(TValue value) => GetSingleMember(value);
	
	#endregion

	/// <summary>
	/// The default value of the enum.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TValue? GetDefaultValue() => default;

	/// <summary>
	/// Get the member count.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetMemberCount() => MemberByNames.Keys.Count;

	/// <summary>
	/// Get the unique member value count.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetUniqueValueCount() => MembersByValues.Keys.Count;

	/// <summary>
	/// Enumerates over the members.
	/// </summary>
	public static IEnumerable<TSelf> GetEnumerable() => MemberByNames.Values;
	
	/// <summary>
	/// Enumerates over the values.
	/// </summary>
	public static IEnumerable<TValue> GetValues() => GetEnumerable().Select(member => member.Value);

	/// <summary>
	/// Is true if the dictionary is in a concurrent state.
	/// </summary>
	protected static bool IsInConcurrentState => MemberByNames is ConcurrentDictionary<string, TSelf>;
	
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
	private static IDictionary<string, TSelf> MemberByNames { get; set; } = new Dictionary<string, TSelf>(StringComparer.OrdinalIgnoreCase);
	
	/// <summary>
	/// A mapping of a member value to one or more members.
	/// </summary>
	private static IDictionary<TValue, IEnumerable<TSelf>> MembersByValues
		=> _membersByValues ??= MemberByNames
			.GroupBy(memberByName => memberByName.Value, memberByName => memberByName.Value)
			.ToDictionary(member => member.Key.Value, member => member.AsEnumerable());
	
	/// <summary>
	/// A lock for the dictionary. 
	/// This lock is used for switching between a concurrent and not-concurrent state (when using <see cref="Core.ConcurrencyMode.AdaptiveConcurrency"/>).
	/// </summary>
	private static readonly object DictionaryLock = new();
	
	/// <summary>
	/// A mapping of a member value to one or more members. Don't change this value. Only reset it (to null).
	/// </summary>
	private static IDictionary<TValue, IEnumerable<TSelf>>? _membersByValues;
	
	static MagicEnumCore()
	{
		ConcurrencyMode = typeof(TSelf).GetCustomAttributes(typeof(DisableConcurrencyAttribute), inherit: true).Any()
			? ConcurrencyMode.NeverConcurrent
			: ConcurrencyMode.AdaptiveConcurrency;

		// Forces to run the static constructor of the user-defined enum, so the Create method is called for every member (in code line order).
		RuntimeHelpers.RunClassConstructor(typeof(TSelf).TypeHandle);

		IsInStaticBuildup = false;
	}
	
	/// <summary>
	/// Gets the last inserted value of the enum.
	/// </summary>
	protected static TValue GetLastInsertedValue() => MembersByValues.LastOrDefault().Key;

	/// <summary>
	/// Creates a new enum member.
	/// </summary>
	/// <param name="value">The value of the new member. Inserting null values is not supported.</param>
	/// <param name="name">
	/// The name of the new member.
	/// Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten. 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected static TSelf CreateMember(TValue value, [CallerMemberName] string name = null!)
	{
		var memberByNames = MemberByNames;

		// Switch to a concurrent mode if necessary.
		if (ConcurrencyMode == ConcurrencyMode.AdaptiveConcurrency && !IsInConcurrentState && !IsInStaticBuildup)
		{
			return SwitchToConcurrentModeAndAddMember(MemberCreator, memberByNames);
		}

		// Create the new member.
		var member = CreateAndAddMemberToDictionary(MemberCreator, memberByNames);
		return member;


		TSelf MemberCreator() => CachedUninitializedMember with { Name = name, Value = value };

		static TSelf SwitchToConcurrentModeAndAddMember(Func<TSelf> memberCreator, IDictionary<string, TSelf> memberByNames)
		{
			lock (DictionaryLock)
			{
				// Check if we didn't win the race.
				// ReSharper disable once PossibleUnintendedReferenceComparison
				if (MemberByNames != memberByNames)
				{
					var member = CreateAndAddMemberToDictionary(memberCreator, MemberByNames);
					return member;
				}

				// Convert to a concurrent dictionary.
				var concurrentMemberByNames = new ConcurrentDictionary<string, TSelf>(memberByNames);
				var newMember = CreateAndAddMemberToDictionary(memberCreator, concurrentMemberByNames);
				MemberByNames = concurrentMemberByNames;
				return newMember;
			}
		}
		
		static TSelf CreateAndAddMemberToDictionary(Func<TSelf> memberCreator, IDictionary<string, TSelf> memberByNames)
		{
			var member = memberCreator();

			// Adds a new member by name.
			if (!memberByNames.TryAdd(member.Name, member)) throw new ArgumentException($"Name '{member.Name}' already defined in enum {typeof(TSelf).Name} (value: '{member.Value}').");
			_membersByValues = null!;

			return member;
		}
	}

	/// <summary>
	/// Used to create new members.
	/// </summary>
	private static readonly TSelf CachedUninitializedMember = (TSelf)FormatterServices.GetUninitializedObject(typeof(TSelf));

	/// <summary>
	/// Tries to retrieve a single member that has the provided name. 
	/// </summary>
	/// <param name="memberName">The name of the member to retrieve.</param>
	/// <param name="member">The queried member.</param>
	/// <returns>True if a member has been found.</returns>
	/// <exception cref="ArgumentNullException">When name is null.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetSingleMember(string memberName, [NotNullWhen(true)] out TSelf? member)
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
	public static TSelf GetSingleMember(string memberName)
		=> TryGetSingleMember(memberName, out var member)
			? member
			: throw new KeyNotFoundException($"Unable to find a member with name '{memberName}' in enum {typeof(TSelf).Name}.");

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
	public static bool TryGetSingleMember(TValue memberValue, [NotNullWhen(true)] out TSelf? member)
	{
		if (memberValue is null) throw new ArgumentNullException(nameof(memberValue));

		if (!MembersByValues.TryGetValue(memberValue, out var members))
		{
			member = default;
			return false;
		}

		var membersList = members.ToList();
		if (membersList.Count > 1) throw new InvalidOperationException($"Expected enum {typeof(TSelf).Name} to have exactly one member with value '{memberValue}'.");

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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TSelf GetSingleMember(TValue memberValue)
		=> TryGetSingleMember(memberValue, out var memberName)
			? memberName
			: throw new KeyNotFoundException($"Unable to find a member with value '{memberValue}' in enum {typeof(TSelf).Name}.");

	/// <summary>
	/// Tries to return the member(s) that have the provided value. 
	/// Use <see cref="TryGetSingleMember(TValue, out TSelf)"/> to retrieve a single member.
	/// </summary>
	/// <param name="memberValue">The value of the member(s) to retrieve.</param>
	/// <param name="members">The queried member(s).</param>
	/// <returns>True if one or more members have been found.</returns>
	/// <exception cref="ArgumentNullException">When value is null.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetMembers(TValue memberValue, out IEnumerable<TSelf> members)
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<TSelf> GetMembers(TValue memberValue)
		=> TryGetMembers(memberValue, out var memberName)
			? memberName
			: throw new KeyNotFoundException($"Unable to find a member with value '{memberValue}' in enum {typeof(TSelf).Name}.");
}