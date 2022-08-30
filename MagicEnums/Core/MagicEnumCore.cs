using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using CodeChops.MagicEnums.Attributes;

namespace CodeChops.MagicEnums.Core;

/// <summary>
/// A magic enum is a flexible and expandable enum which supports other member values than integral types. Inheritance and extensibility are also supported.
/// </summary>
/// <typeparam name="TSelf">The type of the enum itself. Is also the type of each member.</typeparam>
/// <typeparam name="TValue">The type of the enum member value.</typeparam>
public abstract record MagicEnumCore<TSelf, TValue> : Id<TSelf, TValue>, IMagicEnum<TValue>, IEnumerable<TSelf>
	where TSelf : MagicEnumCore<TSelf, TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue>
{
	/// <summary>
	/// Returns the name of the enum.
	/// </summary>
	public sealed override string ToString() => this.ToEasyString(new {this.Name, this.Value});

	/// <summary>
	/// The name of the enum member.
	/// </summary>
	public string Name { get; private init; } = default!;

	#region Comparison
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual bool Equals(MagicEnumCore<TSelf, TValue>? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this.Value, other.Value)) return true;
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
	/// Get the enumerator over the members.
	/// </summary>
	public IEnumerator<TSelf> GetEnumerator() => MemberByNames.Values.GetEnumerator();
	/// <inheritdoc cref="GetEnumerator"/>
	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

	/// <summary>
	/// Get an enumerable over the members.
	/// </summary>
	public static IEnumerable<TSelf> GetMembers() => MemberByNames.Values;
	
	/// <summary>
	/// Get an enumerable over the values.
	/// </summary>
	public static IEnumerable<TValue> GetValues() => MemberByNames.Values.Select(member => member.Value);

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
	private static IDictionary<string, TSelf> MemberByNames { get; set; } = new Dictionary<string, TSelf>(StringComparer.Ordinal);
	
	/// <summary>
	/// A mapping of a member value to one or more members.
	/// </summary>
	private static IDictionary<TValue, List<TSelf>> MembersByValues
		=> _membersByValues ??= MemberByNames
			.GroupBy(memberByName => memberByName.Value, memberByName => memberByName.Value)
			.ToDictionary(member => member.Key.Value, member => member.ToList());
	
	/// <summary>
	/// A mapping of a member value to one or more members. Don't change this value. Only reset it (to null).
	/// </summary>
	private static IDictionary<TValue, List<TSelf>>? _membersByValues;
	
	/// <summary>
	/// A lock for the dictionary. 
	/// This lock is used for switching between a concurrent and not-concurrent state (when using <see cref="Core.ConcurrencyMode.AdaptiveConcurrency"/>).
	/// </summary>
	private static readonly object DictionaryLock = new();

	/// <summary>
	/// A cache for retrieving uninitialized objects of a provided type. Used to create new members. The value is null when the enum is of an abstract type.
	/// </summary>
	private static class CachedUninitializedMember<TMember>
		where TMember : TSelf
	{
		public static TMember Value => _value ?? throw new InvalidOperationException($"Cannot create a MagicEnum member of abstract type {typeof(TSelf).Name}."); 
		private static readonly TMember? _value = typeof(TMember).IsAbstract ? null : (TMember)FormatterServices.GetUninitializedObject(typeof(TMember));
	}

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
	/// Creates a new enum member and returns it..
	/// </summary>
	/// <param name="valueCreator">A function to retrieve the value for the new member.</param>
	/// <param name="memberCreator">Provide this value in order to add enum members that have extra properties.</param>
	/// <param name="name">
	/// The name of the new member.
	/// <b>Do not provide this parameter.</b>
	///<para>
	/// If not provided, the name of the caller of this method will be used as the name of the member.<br/>
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para>
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member already exists with the same name.</exception>
	protected static TMember CreateMember<TMember>(Func<TValue> valueCreator, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!)
		where TMember : TSelf
		=> CreateAndAddMember(name, valueCreator, throwWhenExists: true, memberCreator);
	
	/// <summary>
	/// Creates a new enum member if it does not exist and returns it. When it already exists, it returns the member with the same name.
	/// </summary>
	/// <param name="valueCreator">A function to retrieve the value for the new member.</param>
	/// <param name="memberCreator">Provide this value in order to add enum members that have extra properties.</param>
	/// <param name="name">
	/// The name of the new member.
	/// <b>Do not provide this parameter.</b>
	///<para>
	/// If not provided, the name of the caller of this method will be used as the name of the member.<br/>
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para>
	/// </param>
	/// <returns>The newly created member or an existing enum member with the same name.</returns>
	protected static TSelf GetOrCreateMember(Func<TValue> valueCreator, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
		=> CreateAndAddMember(name, valueCreator, throwWhenExists: false, memberCreator);
	
	/// <summary>
	/// Creates a member and adds it to the dictionary.
	/// </summary>
	/// <param name="name">The member name.</param>
	/// <param name="valueCreator">A function to retrieve the value for the new member.</param>
	/// <param name="throwWhenExists">Throw when a member of the same name already exists.</param>
	/// <param name="memberCreator">Provide this value in order to add enum members that have extra properties.</param>
	/// <returns>The newly created enum member.</returns>
	/// <exception cref="ArgumentException">When the member exists and <paramref name="throwWhenExists"/> is true.</exception>
	private static TMember CreateAndAddMember<TMember>(string name, Func<TValue> valueCreator, bool throwWhenExists, Func<TMember>? memberCreator = null)
		where TMember : TSelf
	{
		// Copy the reference so we don't get race conditions.
		var memberByNames = MemberByNames;

		// Switch to a concurrent mode if necessary.
		return ConcurrencyMode == ConcurrencyMode.AdaptiveConcurrency && !IsInConcurrentState && !IsInStaticBuildup
			? SwitchToConcurrentModeAndAddMember(memberByNames)
			: CreateMemberAndAddToDictionary(memberByNames, IsInConcurrentState);


		TMember SwitchToConcurrentModeAndAddMember(IDictionary<string, TSelf> memberByNames)
		{
			lock (DictionaryLock)
			{
				// Check if we didn't win the race.
				// ReSharper disable once PossibleUnintendedReferenceComparison
				if (MemberByNames != memberByNames)
					return CreateMemberAndAddToDictionary(MemberByNames, isInConcurrentState: true);

				// Convert to a concurrent dictionary.
				var concurrentMemberByNames = new ConcurrentDictionary<string, TSelf>(memberByNames);
				var newMember = CreateMemberAndAddToDictionary(concurrentMemberByNames, isInConcurrentState: true);
				MemberByNames = concurrentMemberByNames;
				return newMember;
			}
		}
		
		TMember CreateMemberAndAddToDictionary(IDictionary<string, TSelf> memberByNames, bool isInConcurrentState)
		{
			// Adds a new member by name.
			if (memberByNames.TryGetValue(name, out var member))
			{
				if (throwWhenExists)
					throw new ArgumentException($"Name '{name}' already defined in enum {typeof(TSelf).Name} (value: '{member.Value}').");

				return (TMember)member;
			}
			
			// Create the member
			member = memberCreator?.Invoke() ?? CachedUninitializedMember<TMember>.Value;
			member = member with { Name = name, Value = valueCreator() };
			
			// Add the new member
			if (isInConcurrentState) ((ConcurrentDictionary<string, TSelf>)memberByNames).TryAdd(member.Name, member);
			else memberByNames.Add(member.Name, member);
			
			// Reset the backing field of MembersByValues, in order to repopulate it, based on the new members.
			_membersByValues = null!;
	
			return (TMember)member;
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
		if (membersList.Count > 1) throw new InvalidOperationException($"Expected enum {typeof(TSelf).Name} to have exactly one member with value '{memberValue}', but it has {membersList.Count} members.");

		member = membersList.First();
		return true;
	}

	/// <summary>
	/// Returns the single member that has the provided value. Throws when no member has been found.
	/// Warning! Multiple members can have the same value. This method will throw when multiple members have this value.
	/// Use <see cref="GetMembers(TValue)"/> to retrieve multiple members.
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
	public static bool TryGetMembers(TValue memberValue, [NotNullWhen(true)] out IReadOnlyCollection<TSelf>? members)
	{
		if (memberValue is null) throw new ArgumentNullException(nameof(memberValue));
		if (MembersByValues.TryGetValue(memberValue, out var membersList))
		{
			members = membersList;
			return true;
		}

		members = null;
		return false;
	}

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