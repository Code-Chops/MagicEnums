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
public abstract record MagicEnumCore<TSelf, TValue> : Id<TSelf, TValue>, IMagicEnum<TValue>, IEnumerable<TSelf>, IComparable<MagicEnumCore<TSelf, TValue>>
	where TSelf : MagicEnumCore<TSelf, TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue>
{
	/// <summary>
	/// Returns the name of the enum.
	/// </summary>
	public sealed override string ToString() => $"{typeof(TSelf).Name} {{ {nameof(this.Name)} = {this.Name}, {nameof(this.Value)} = {this.Value} }}";

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
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(MagicEnumCore<TSelf, TValue>? other)
	{
		if (other is null) throw new ArgumentNullException(nameof(other));
		return this.Value.CompareTo(other.Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <	(MagicEnumCore<TSelf, TValue> left, MagicEnumCore<TSelf, TValue> right)	=> left.CompareTo(right) <	0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <=	(MagicEnumCore<TSelf, TValue> left, MagicEnumCore<TSelf, TValue> right)	=> left.CompareTo(right) <= 0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >	(MagicEnumCore<TSelf, TValue> left, MagicEnumCore<TSelf, TValue> right)	=> left.CompareTo(right) >	0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >=	(MagicEnumCore<TSelf, TValue> left, MagicEnumCore<TSelf, TValue> right)	=> left.CompareTo(right) >= 0;
	
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
	/// Get the enumerator over the member values.
	/// </summary>
	public IEnumerator<TSelf> GetEnumerator() => MemberByNames.Values.GetEnumerator();
	/// <inheritdoc cref="GetEnumerator"/>
	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

	/// <summary>
	/// Get an enumerable over the member values.
	/// </summary>
	public static IEnumerable<TSelf> GetEnumerable() => MemberByNames.Values;

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
	/// Creates a new enum member and returns it.
	/// </summary>
	/// <param name="value">The value of the new member.</param>
	/// <param name="name">
	/// The name of the new member.
	/// <para>
	/// Warning: Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para> 
	/// </param>
	/// <returns>The newly created member.</returns>
	/// <exception cref="ArgumentException">When a member with the same name already exists.</exception>
	protected static TSelf CreateMember(TValue value, [CallerMemberName] string name = null!)
		=> CreateAndAddMember(name, value, throwWhenExists: true);
	
	/// <summary>
	/// Creates a new enum member if it does not exist and returns it. When it already exists, it returns the member with the same name.
	/// </summary>
	/// <param name="value">The value of the new member.</param>
	/// <param name="name">
	/// The name of the member.
	/// <para>
	/// Warning: Don't provide this parameter, so the property name of the enum will automatically be used as the name of the member. 
	/// If provided, the enforced name will be used, and the property name the will be forgotten.
	/// </para> 
	/// </param>
	/// <returns>The newly created member or an existing enum member with the same name.</returns>
	protected static TSelf GetOrCreateMember(TValue value, [CallerMemberName] string name = null!)
		=> CreateAndAddMember(name, value, throwWhenExists: false);
	
	/// <summary>
	/// Creates a member based on <see cref="CachedUninitializedMember"/>.
	/// </summary>
	/// <param name="name">The member name.</param>
	/// <param name="value">The member value.</param>
	/// <returns>The enum.</returns>
	/// <exception cref="InvalidOperationException">When the enum is of an abstract type.</exception>
	private static TSelf MemberFactory(string name, TValue value)
	{
		if (CachedUninitializedMember is null) throw new InvalidOperationException($"Cannot create a MagicEnum member of abstract type {typeof(TSelf).Name}.");
		return CachedUninitializedMember with { Name = name, _value = value };
	}

	/// <summary>
	/// Creates a member and adds it to the dictionary.
	/// </summary>
	/// <param name="name">The member name.</param>
	/// <param name="value">The member value.</param>
	/// <param name="throwWhenExists">Throw when a member of the same name already exists.</param>
	/// <returns>The enum.</returns>
	/// <exception cref="ArgumentException">When the member exists and <paramref name="throwWhenExists"/> is true.</exception>
	private static TSelf CreateAndAddMember(string name, TValue value, bool throwWhenExists)
	{
		var memberByNames = MemberByNames;

		// Switch to a concurrent mode if necessary.
		if (ConcurrencyMode == ConcurrencyMode.AdaptiveConcurrency && !IsInConcurrentState && !IsInStaticBuildup)
			return SwitchToConcurrentModeAndAddMember(memberByNames);

		// Otherwise, create the new member.
		var member = CreateMemberAndAddToDictionary(memberByNames, IsInConcurrentState);
		return member;


		TSelf SwitchToConcurrentModeAndAddMember(IDictionary<string, TSelf> memberByNames)
		{
			lock (DictionaryLock)
			{
				// Check if we didn't win the race.
				// ReSharper disable once PossibleUnintendedReferenceComparison
				if (MemberByNames != memberByNames)
					return CreateMemberAndAddToDictionary(MemberByNames, isInConcurrentState: false);

				// Convert to a concurrent dictionary.
				var concurrentMemberByNames = new ConcurrentDictionary<string, TSelf>(memberByNames);
				var newMember = CreateMemberAndAddToDictionary(concurrentMemberByNames, isInConcurrentState: true);
				MemberByNames = concurrentMemberByNames;
				return newMember;
			}
		}
		
		TSelf CreateMemberAndAddToDictionary(IDictionary<string, TSelf> memberByNames, bool isInConcurrentState)
		{
			// Adds a new member by name.
			if (memberByNames.TryGetValue(name, out var member))
			{
				if (throwWhenExists)
					throw new ArgumentException($"Name '{name}' already defined in enum {typeof(TSelf).Name} (value: '{member.Value}').");

				return member;
			}
			
			member = MemberFactory(name, value);
			
			if (isInConcurrentState) 
				((ConcurrentDictionary<string, TSelf>)memberByNames).TryAdd(member.Name, member);
			else 
				memberByNames.Add(member.Name, member);
			
			_membersByValues = null!;
	
			return member;
		}
	}

	/// <summary>
	/// Used to create new members. Is null when the enum is of an abstract type.
	/// </summary>
	private static readonly TSelf? CachedUninitializedMember = typeof(TSelf).IsAbstract ? null : (TSelf)FormatterServices.GetUninitializedObject(typeof(TSelf));

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