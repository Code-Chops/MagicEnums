﻿namespace CodeChops.MagicEnums.Core;

/// <summary>
/// A magic enum is a flexible and expandable enum which supports other member values than integral types.
/// Inheritance and extensibility are also supported.
/// </summary>
/// <typeparam name="TSelf">The type of the enum itself. Is also the type of each member.</typeparam>
/// <typeparam name="TValue">The type of the enum member value.</typeparam>
public abstract record MagicEnumCore<TSelf, TValue> : IMagicEnumCore<TSelf, TValue>, IComparable<TSelf>, IComparable
    where TSelf : MagicEnumCore<TSelf, TValue>
    where TValue : IEquatable<TValue?>, IComparable<TValue>
{
    public sealed override string ToString() => this.Name;

    /// <summary>
    /// The name of the enum member.
    /// </summary>
    public string Name { get; private init; } = null!;
    public TValue? Value { get; private init; }

    #region Comparison
    public int CompareTo(TSelf? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Comparer<TValue?>.Default.Compare(this.Value, other.Value);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        return obj is MagicEnumCore<TSelf, TValue> other ? this.CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(MagicEnumCore<TSelf, TValue>)}");
    }

    public static bool operator <(MagicEnumCore<TSelf, TValue> left, TSelf? right) => left.CompareTo(right) < 0;
    public static bool operator >(MagicEnumCore<TSelf, TValue> left, TSelf? right) => left.CompareTo(right) > 0;
    public static bool operator <=(MagicEnumCore<TSelf, TValue> left, TSelf? right) => left.CompareTo(right) <= 0;
    public static bool operator >=(MagicEnumCore<TSelf, TValue> left, TSelf? right) => left.CompareTo(right) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool Equals(MagicEnumCore<TSelf, TValue>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this.Value, other.Value)) return true;
        return this.Value?.Equals(other.Value) ?? other.Value is null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => this.Value!.GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator TValue?(MagicEnumCore<TSelf, TValue>? magicEnum)
        => magicEnum is null ? default : magicEnum.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator MagicEnumCore<TSelf, TValue>(TValue value)
        => GetSingleMember(value);

    #endregion

    /// <summary>
    /// Get the default value of the enum.
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
    /// Get an enumerator over the members.
    /// </summary>
    public IEnumerator<TSelf> GetEnumerator() => MemberByNames.Values.GetEnumerator();
    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// Get an enumerable over the members.
    /// </summary>
    public static IEnumerable<TSelf> GetMembers()
        => MemberByNames.Values;

    /// <summary>
    /// Get an enumerable over the values.
    /// </summary>
    public static IEnumerable<TValue> GetValues()
        => MemberByNames
            .Values
            .Where(member => member.Value is not null)
            .Select(member => member.Value!);

    /// <summary>
    /// Is true if the dictionary is in a concurrent state.
    /// </summary>
    private protected static bool IsInConcurrentState => MemberByNames is ConcurrentDictionary<string, TSelf>;

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
            .Where(memberByName => memberByName.Value.Value is not null)
            .GroupBy(memberByName => memberByName.Value, memberByName => memberByName.Value)
            .ToDictionary(member => member.Key.Value!, member => member.ToList());

    /// <summary>
    /// A mapping of a member value to one or more members. Don't change this value. Only reset it (to null).
    /// </summary>
    private static IDictionary<TValue, List<TSelf>>? _membersByValues;

    /// <summary>
    /// A lock used for switching between concurrent and non-concurrent states (when using <see cref="Core.ConcurrencyMode.AdaptiveConcurrency"/>).
    /// </summary>
    private static readonly Lock DictionaryLock = new();

    /// <summary>
    /// A cache for retrieving uninitialized objects of a provided type. Used to create new members without a provided memberCreator.
    /// </summary>
    /// <typeparam name="TMember">The type of the uninitialized member to create.</typeparam>
    /// <exception cref="InvalidOperationException">When <typeparamref name="TMember"/> is abstract.</exception>
    private static class CachedUninitializedMember<TMember>
        where TMember : TSelf
    {
        /// <summary>
        /// Returns the cached uninitialized member. Throws when the member is of an abstract type.
        /// </summary>
        /// <returns>Returns the cached uninitialized member.</returns>
        /// <exception cref="InvalidOperationException">When TMember is an abstract type.</exception>
        public static TMember GetValue() => Value ?? throw new InvalidOperationException($"Cannot create a MagicEnum member of abstract type {typeof(TMember).Name} for enum {typeof(TSelf).Name}.");
        private static readonly TMember? Value = typeof(TMember).IsAbstract ? null : (TMember)RuntimeHelpers.GetUninitializedObject(typeof(TMember));
    }

    static MagicEnumCore()
    {
        ConcurrencyMode = typeof(TSelf).GetCustomAttributes(typeof(DisableConcurrencyAttribute), inherit: true).Length is not 0
            ? ConcurrencyMode.NeverConcurrent
            : ConcurrencyMode.AdaptiveConcurrency;

        // Forces to run the static constructor of the user-defined enum, so the Create method is called for every member (in code line order).
        RuntimeHelpers.RunClassConstructor(typeof(TSelf).TypeHandle);

        IsInStaticBuildup = false;
    }

    protected MagicEnumCore()
    {
        // When instantiating the enum (not creating a member), set the name of enum.
        this.Name = typeof(TSelf).Name;
    }

    /// <summary>
    /// Creates a new enum member and returns it.
    /// </summary>
    /// <param name="valueCreator">A function to retrieve the value for the new member.</param>
    /// <param name="memberCreator">Optional: A function to construct subtypes without parameterless constructors.</param>
    /// <param name="name">
    /// The name of the new member.
    /// <b>Do not provide this parameter!</b>
    ///<para>
    /// If not provided, the name of the caller of this method will be used as the name of the member.<br/>
    /// If provided, the enforced name will be used, and the property name the will be forgotten.
    /// </para>
    /// </param>
    /// <typeparam name="TMember">The type of the new member (which should be the type of the enum or a subtype of it).</typeparam>
    /// <returns>The newly created member.</returns>
    /// <exception cref="InvalidOperationException">When a member with the same name already exists.</exception>
    /// <exception cref="ArgumentNullException">When the member name argument is null.</exception>
    protected static TMember CreateMember<TMember>(Func<TValue> valueCreator, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!)
        where TMember : TSelf
        => CreateAndAddMember(name, valueCreator, throwWhenExists: true, memberCreator);

    /// <summary>
    /// Creates a new enum member if it does not exist and returns it. If it already exists, it returns the member with the same name.
    /// </summary>
    /// <param name="name">The name of the new member.</param>
    /// <param name="valueCreator">A function to retrieve the value for the new member.</param>
    /// <param name="memberCreator">Optional: A function to construct subtypes without parameterless constructors.</param>
    /// <typeparam name="TMember">The type of the new member (which should be the type of the enum or a subtype of it).</typeparam>
    /// <returns>The newly created member or an existing enum member with the same name.</returns>
    /// <exception cref="ArgumentNullException">When the member name argument is null.</exception>
    protected static TMember GetOrCreateMember<TMember>(string name, Func<TValue> valueCreator, Func<TMember>? memberCreator = null)
        where TMember : TSelf
        => CreateAndAddMember(name, valueCreator, throwWhenExists: false, memberCreator);

    /// <summary>
    /// Creates a member and adds it to the dictionary.
    /// </summary>
    /// <param name="name">The name of the new member.</param>
    /// <param name="valueCreator">A function to retrieve the value for the new member.</param>
    /// <param name="throwWhenExists">Throw when a member of the same name already exists.</param>
    /// <param name="memberCreator">Optional: A function to construct subtypes without parameterless constructors.</param>
    /// <returns>The newly created enum member.</returns>
    /// <exception cref="InvalidOperationException">When a member with the name already exists and <paramref name="throwWhenExists"/> is true.</exception>
    /// <exception cref="ArgumentNullException">When the member name or valueCreator argument is null.</exception>
    private static TMember CreateAndAddMember<TMember>(string name, Func<TValue> valueCreator, bool throwWhenExists, Func<TMember>? memberCreator = null)
        where TMember : TSelf
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(valueCreator);

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
                    throw new InvalidOperationException($"Member with name '{name}' already defined in MagicEnum {typeof(TSelf).Name} (value: '{member.Value}').");

                return (TMember)member;
            }

            // Create the member
            member = memberCreator?.Invoke() ?? CachedUninitializedMember<TMember>.GetValue();
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
    /// Tries to retrieve a single member with the provided name.
    /// </summary>
    /// <param name="memberName">The name of the member to retrieve.</param>
    /// <param name="member">The found member.</param>
    /// <returns>True if a member has been found, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">When the member name argument is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetSingleMember(string memberName, [NotNullWhen(true)] out TSelf? member)
    {
        ArgumentNullException.ThrowIfNull(memberName);

        return MemberByNames.TryGetValue(memberName, out member);
    }

    /// <summary>
    /// Returns a single member with the provided name.
    /// <para><b>Throws when no member has been found.</b></para>
    /// </summary>
    /// <param name="memberName">The name of the member to retrieve.</param>
    /// <returns>The found member.</returns>
    /// <exception cref="ArgumentNullException">When the member name argument is null.</exception>
    /// <exception cref="KeyNotFoundException">When no member has been found.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf GetSingleMember(string memberName)
    {
        ArgumentNullException.ThrowIfNull(memberName);

        return TryGetSingleMember(memberName, out var member)
            ? member
            : throw new KeyNotFoundException($"Unable to find a member with name '{memberName}' in MagicEnum {typeof(TSelf).Name}.");
    }

    /// <summary>
    /// Tries to return the single member with the provided value.
    /// <para><b>Throws when multiple members have this value.</b></para>
    /// Use <see cref="TryGetMembers(TValue, out IReadOnlyCollection{TSelf}?)"/> to try to retrieve multiple members.
    /// </summary>
    /// <param name="memberValue">The value of the member to retrieve.</param>
    /// <param name="member">The found member.</param>
    /// <returns>True if a single member has been found, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">When the member value argument is null.</exception>
    /// <exception cref="InvalidOperationException">When multiple members with the same value have been found.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetSingleMember(TValue memberValue, [NotNullWhen(true)] out TSelf? member)
    {
        ArgumentNullException.ThrowIfNull(memberValue);

        if (!MembersByValues.TryGetValue(memberValue, out var members))
        {
            member = null;
            return false;
        }

        var membersList = members.ToList();
        if (membersList.Count > 1)
            throw new InvalidOperationException($"Expected MagicEnum {typeof(TSelf).Name} to have exactly one member with value '{memberValue}', but it has {membersList.Count} members.");

        member = membersList.First();
        return true;
    }

    /// <summary>
    /// Returns the single member with the provided value.
    /// <para><b>
    /// Throws when no member has been found.<br/>
    /// Throws when multiple members are found.
    /// </b></para>
    /// Use <see cref="GetMembers(TValue)"/> to retrieve multiple members.
    /// </summary>
    /// <param name="memberValue">The value of the member to retrieve.</param>
    /// <returns>The found member.</returns>
    /// <exception cref="ArgumentNullException">When the member value argument is null.</exception>
    /// <exception cref="InvalidOperationException">When multiple members with the same value have been found.</exception>
    /// <exception cref="KeyNotFoundException">When no member has been found.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf GetSingleMember(TValue memberValue)
    {
        ArgumentNullException.ThrowIfNull(memberValue);

        return TryGetSingleMember(memberValue, out var memberName)
            ? memberName
            : throw new KeyNotFoundException($"Unable to find a member with value '{memberValue}' in MagicEnum {typeof(TSelf).Name}.");
    }

    /// <summary>
    /// Tries to return the member(s) with the provided value.
    /// Use <see cref="TryGetSingleMember(TValue, out TSelf)"/> to try to retrieve a single member.
    /// </summary>
    /// <param name="memberValue">The value of the member(s) to retrieve.</param>
    /// <param name="members">The found member(s).</param>
    /// <returns>True if one or more members have been found.</returns>
    /// <exception cref="ArgumentNullException">>When the member value argument is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetMembers(TValue memberValue, [NotNullWhen(true)] out IReadOnlyCollection<TSelf>? members)
    {
        ArgumentNullException.ThrowIfNull(memberValue);

        if (MembersByValues.TryGetValue(memberValue, out var membersList))
        {
            members = membersList;
            return true;
        }

        members = null;
        return false;
    }

    /// <summary>
    /// Returns the member(s) with the provided value.
    /// <para><b>Throws when no member has been found.</b></para>
    /// Use <see cref="GetSingleMember(TValue)"/> to retrieve a single member.
    /// </summary>
    /// <param name="memberValue">The value of the member(s) to retrieve.</param>
    /// <returns>The found member(s).</returns>
    /// <exception cref="ArgumentNullException">When the member value argument is null.</exception>
    /// <exception cref="KeyNotFoundException">When no member has been found.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TSelf> GetMembers(TValue memberValue)
    {
        ArgumentNullException.ThrowIfNull(memberValue);

        return TryGetMembers(memberValue, out var memberName)
            ? memberName
            : throw new KeyNotFoundException(
                $"Unable to find a member with value '{memberValue}' in MagicEnum {typeof(TSelf).Name}.");
    }


    #region ForwardInstancesMethodsToStatic

    /// <inheritdoc cref="GetDefaultValue"/>
    TValue? IMagicEnum<TValue>.GetDefaultValueFromInstance() => GetDefaultValue();

    /// <inheritdoc cref="GetMemberCount"/>
    int IMagicEnum<TValue>.GetMemberCountFromInstance() => GetMemberCount();

    /// <inheritdoc cref="GetUniqueValueCount"/>
    int IMagicEnum<TValue>.GetUniqueValueCountFromInstance() => GetUniqueValueCount();

    /// <inheritdoc cref="GetMembers()"/>
    IEnumerable<IMagicEnum<TValue>> IMagicEnum<TValue>.GetMembersFromInstance() => GetMembers();

    /// <inheritdoc cref="GetValues()"/>
    IEnumerable<TValue> IMagicEnum<TValue>.GetValuesFromInstance() => GetValues();

    /// <inheritdoc cref="TryGetSingleMember(string, out TSelf)"/>
    bool IMagicEnum<TValue>.TryGetSingleMemberFromInstance(string memberName, [NotNullWhen(true)] out IMagicEnum<TValue>? member)
    {
        if (!TryGetSingleMember(memberName, out var foundMember))
        {
            member = null;
            return false;
        }

        member = foundMember;
        return true;
    }

    /// <inheritdoc cref="GetSingleMember(string)"/>
    IMagicEnum<TValue> IMagicEnum<TValue>.GetSingleMemberFromInstance(string memberName) => GetSingleMember(memberName);

    /// <inheritdoc cref="TryGetSingleMember(TValue, out TSelf?)"/>
    bool IMagicEnum<TValue>.TryGetSingleMemberFromInstance(TValue memberValue, [NotNullWhen(true)] out IMagicEnum<TValue>? member)
    {
        if (!TryGetSingleMember(memberValue, out var foundMember))
        {
            member = null;
            return false;
        }

        member = foundMember;
        return true;
    }

    /// <inheritdoc cref="GetSingleMember(TValue)"/>
    IMagicEnum<TValue> IMagicEnum<TValue>.GetSingleMemberFromInstance(TValue memberValue) => GetSingleMember(memberValue);

    /// <inheritdoc cref="TryGetMembers(TValue, out IReadOnlyCollection{TSelf}?)"/>
    bool IMagicEnum<TValue>.TryGetMembersFromInstance(TValue memberValue, [NotNullWhen(true)] out IReadOnlyCollection<IMagicEnum<TValue>>? members)
    {
        if (!TryGetMembers(memberValue, out var foundMembers))
        {
            members = null;
            return false;
        }

        members = foundMembers;
        return true;
    }

    /// <inheritdoc cref="GetMembers(TValue)"/>
    IEnumerable<IMagicEnum<TValue>> IMagicEnum<TValue>.GetMembersFromInstance(TValue memberValue) => GetMembers(memberValue);

    #endregion
}