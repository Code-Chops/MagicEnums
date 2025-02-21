using System.Numerics;

namespace CodeChops.MagicEnums;

/// <summary>
/// An enum with integer values.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
public abstract record MagicEnum<TSelf> : MagicEnum<TSelf, int>
    where TSelf : MagicEnum<TSelf>;

/// <summary>
/// An enum with integral values.
/// </summary>
/// <typeparam name="TSelf">The type of the number enum itself. Is also equal to the type of each member.</typeparam>
/// <typeparam name="TValue">The integral type.</typeparam>
public abstract record MagicEnum<TSelf, TValue> : MagicEnumCore<TSelf, TValue>
    where TSelf : MagicEnum<TSelf, TValue>
    where TValue : struct, INumber<TValue>
{
    #region LastInsertedNumber

    /// <summary>
    /// The value of the latest inserted enum member (starts with null).
    /// Used for auto-incrementing the value of a new member when no value has been provided (implicit value).
    /// </summary>
    private static TValue? LastInsertedNumber { get; set; }

    /// <summary>
    /// Locks the retrieval and incrementation of the last inserted number.
    /// </summary>
    private static readonly object LockLastInsertedNumber = new();

    /// <summary>
    /// Sets and retrieves the last number (during a lock, if necessary).
    /// </summary>
    private static TValue SetAndRetrieveLastInsertedNumber(Func<TValue> valueCreator)
    {
        if (IsInConcurrentState)
        {
            lock (LockLastInsertedNumber)
            {
                LastInsertedNumber = valueCreator();
                return (TValue)LastInsertedNumber;
            }
        }

        LastInsertedNumber = valueCreator();
        return (TValue)LastInsertedNumber;
    }

    /// <summary>
    /// Increments the last inserted enum value and returns it.
    /// </summary>
    private static TValue GetIncrementedLastInsertedNumber() => LastInsertedNumber is null
        ? TValue.Zero
        : (TValue)LastInsertedNumber + TValue.One;

    #endregion

    #region CreateMember

    // Creates and returns a new enum member of the same type as the enum itself.
    /// <inheritdoc cref="CreateMember{TMember}(Func{TMember}?, TValue?, string)"/>
    protected static TSelf CreateMember(TValue? value = null, Func<TSelf>? memberCreator = null, [CallerMemberName] string name = null!)
        => CreateMember(memberCreator, value, name);

    /// <summary>
    /// Creates a new enum member and returns it.
    /// </summary>
    /// <param name="value">The value of the new member. If not provided, the last inserted enum value will be incremented and used.</param>
    /// <param name="memberCreator">Optional: A function to construct subtypes without parameterless constructors.</param>
    /// <param name="name">
    /// The name of the new member.
    /// <b>Do not provide this parameter!</b>
    ///<para>
    /// If not provided, the name of the caller of this method will be used as the name of the member.<br/>
    /// If provided, the enforced name will be used, and the property name the will be forgotten.
    /// </para>
    /// </param>
    /// <returns>The newly created member.</returns>
    /// <exception cref="InvalidOperationException">When a member with the same name already exists.</exception>
    /// <exception cref="ArgumentNullException">When the member name argument is null.</exception>
    protected static TMember CreateMember<TMember>(Func<TMember>? memberCreator = null, TValue? value = null, [CallerMemberName] string name = null!)
        where TMember : TSelf
        => CreateMember(
            valueCreator: value is null
                ? GetIncrementedLastInsertedNumber
                : () => value.Value,
            memberCreator: memberCreator,
            name: name);

    // Hides the magic enum core method in order to force saving the last inserted number.
    /// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.CreateMember{TMember}"/>
    protected new static TMember CreateMember<TMember>(Func<TValue> valueCreator, Func<TMember>? memberCreator = null, [CallerMemberName] string name = null!)
        where TMember : TSelf
        => MagicEnumCore<TSelf, TValue>.CreateMember(
            valueCreator: () => SetAndRetrieveLastInsertedNumber(valueCreator),
            memberCreator: memberCreator,
            name: name);

    #endregion

    #region GetOrCreateMember

    // Creates or retrieves a new enum member of the same type as the enum itself.
    /// <inheritdoc cref="GetOrCreateMember{TMember}(string, TValue, Func{TMember}?)"/>
    protected static TSelf GetOrCreateMember(string name, TValue value, Func<TSelf>? memberCreator = null)
        => GetOrCreateMember<TSelf>(name, value, memberCreator);

    /// <summary>
    /// Creates a new enum member with the provided integral value, or gets an existing member of the provided name.
    /// </summary>
    /// <param name="name">The name of the new member.</param>
    /// <param name="value">The value of the new member.</param>
    /// <param name="memberCreator">Optional: A function to construct subtypes without parameterless constructors.</param>
    /// <returns>The newly created member.</returns>
    /// <exception cref="ArgumentNullException">When the member name argument is null.</exception>
    protected static TMember GetOrCreateMember<TMember>(string name, TValue value, Func<TMember>? memberCreator = null)
        where TMember : TSelf
        => GetOrCreateMember(
            name: name,
            valueCreator:() => SetAndRetrieveLastInsertedNumber(() => value),
            memberCreator: memberCreator);

    // Hides the magic enum core method in order to force saving the last inserted number.
    /// <inheritdoc cref="MagicEnumCore{TSelf, TValue}.GetOrCreateMember{TMember}"/>
    protected new static TMember GetOrCreateMember<TMember>(string name, Func<TValue> valueCreator, Func<TMember>? memberCreator = null)
        where TMember : TSelf
        => MagicEnumCore<TSelf, TValue>.GetOrCreateMember(
            name: name,
            valueCreator: () => SetAndRetrieveLastInsertedNumber(valueCreator),
            memberCreator: memberCreator);

    #endregion
}