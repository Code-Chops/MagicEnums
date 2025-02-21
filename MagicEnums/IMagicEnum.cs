namespace CodeChops.MagicEnums;

public interface IMagicEnum<TValue> : IMember<TValue>, IMagicEnum
    where TValue : IEquatable<TValue>, IComparable<TValue>
{
    /// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.GetDefaultValue()"/>
    TValue? GetDefaultValueFromInstance();

    /// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.GetMemberCount()"/>
    int GetMemberCountFromInstance();

    /// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.GetUniqueValueCount()"/>
    int GetUniqueValueCountFromInstance();

    /// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.GetMembers()"/>
    IEnumerable<IMagicEnum<TValue>> GetMembersFromInstance();

    /// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.GetValues()"/>
    IEnumerable<TValue> GetValuesFromInstance();

    /// <summary>
    /// /// Tries to retrieve a single member with the provided name.
    /// </summary>
    /// <param name="memberName">The name of the member to retrieve.</param>
    /// <param name="member">The found member.</param>
    /// <returns>True if a member has been found, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">When the member name argument is null.</exception>
    bool TryGetSingleMemberFromInstance(string memberName, [NotNullWhen(true)] out IMagicEnum<TValue>? member);

    /// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.GetSingleMember(string)"/>
    IMagicEnum<TValue> GetSingleMemberFromInstance(string memberName);

    /// <summary>
    /// Tries to return the single member with the provided value.
    /// <para><b>Throws when multiple members have this value.</b></para>
    /// Use <see cref="TryGetMembersFromInstance(TValue, out IReadOnlyCollection{IMagicEnum{TValue}}?)"/> to try to retrieve multiple members.
    /// </summary>
    /// <param name="memberValue">The value of the member to retrieve.</param>
    /// <param name="member">The found member.</param>
    /// <returns>True if a single member has been found, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">When the member value argument is null.</exception>
    /// <exception cref="InvalidOperationException">When multiple members with the same value have been found.</exception>
    bool TryGetSingleMemberFromInstance(TValue memberValue, [NotNullWhen(true)] out IMagicEnum<TValue>? member);

    /// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.GetSingleMember(TValue)"/>
    IMagicEnum<TValue> GetSingleMemberFromInstance(TValue memberValue);

    /// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.TryGetMembers(TValue, out IReadOnlyCollection{TSelf}?)"/>
    bool TryGetMembersFromInstance(TValue memberValue, [NotNullWhen(true)] out IReadOnlyCollection<IMagicEnum<TValue>>? members);

    /// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.GetMembers(TValue)"/>
    IEnumerable<IMagicEnum<TValue>> GetMembersFromInstance(TValue memberValue);
}

public interface IMagicEnum : IMember;