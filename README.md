# Magic Enums

Customizable, and extendable enums for C# with a clean API.

> Check out [CodeChops projects](https://www.CodeChops.nl/projects) for more projects.

# Examples

## Simple enum
```csharp
// Implicit values
public record Level : MagicEnum<Level>
{
    public static readonly Level Low    = CreateMember(); // 0
    public static readonly Level Medium = CreateMember(); // 1
    public static readonly Level High   = CreateMember(); // 2
}

// Explicit values
public record StarRating : MagicEnum<StarRating>
{
    public static readonly StarRating One   = CreateMember(1);
    public static readonly StarRating Two   = CreateMember();    
    public static readonly StarRating Three = CreateMember();
    public static readonly StarRating Four  = CreateMember();
    public static readonly StarRating Five  = CreateMember();
}
```

## Complex enum
```csharp
public record Vehicle(int WheelCount) : MagicEnum<Vehicle>
{
    public static readonly Type.Bicycle    Bicycle     = CreateMember<Type.Bicycle>();
    public static readonly Type.MotorCycle MotorCycle  = CreateMember<Type.MotorCycle>();
    public static readonly Type.Car        FuelCar     = CreateMember<Type.Car>(() => new(EmitsCo2: true));
    public static readonly Type.Car        ElectricCar = CreateMember<Type.Car>(() => new(EmitsCo2: false));
    
    public static class Type
    {
        public record Bicycle()          : Vehicle(WheelCount: 2);
        public record MotorCycle()       : Vehicle(WheelCount: 2);
        public record Car(bool EmitsCo2) : Vehicle(WheelCount: 4);
    } 
}
``` 

## Advantages
Besides the default .NET enum behaviour, MagicEnums offer more features than the default .NET enum implementation:
- Fast and optimized: does **not use reflection**!
- Extendability:
  - **Inheritance** is supported. This way enums can also be extended in other assemblies.
  - **Partial enums** are supported.
  - **Custom methods and properties** can be added to the enums.
- **Different types of enums are supported**:
  - [Number enums](#Number-enums) (including `decimal`)
  - [Flags enums](#Flags-enums)
  - [String enums](#String-enums)
  - [Custom enums](#Custom-enums)
- Members can be **added at runtime**, if necessary. This is thread-safe.
- **Members with the same value can be looked up easily**. Something which is not supported in default C# enums.
- **Optimized**, and therefore fast member registration and lookup, including a fast `ToString`. For extra optimization, see [optimization](#Optimization).
- Serialization to/from **JSON** is supported.
- **Members can be auto-discovered**. This removes the need to keep track of used/unused enum-members. See [auto member discoverability](#Member-discoverability).

# Functionality
> **Terminology:**
> - An `enum` has one or multiple members.
> - Each `member` has a `name` and a `value`.
> - The type of the value depends on the chosen `enum type`, see: [enum types](#Enum-types).

Magic enums behave like the default .NET enum implementation:
- They use `int` as default for the member value.
- Members can be found by searching for their name or value.
- More than one member can have the same value but not the same name.
- The value of members can be omitted (when using the default numeric enums). If omitted, it automatically increments the value of each member.
- Members, member names or member values can easily be enumerated.
- `Flag` enums are supported.
- Strongly typed enum members, so pattern matching can be used.

# Usage
1. Add the package `CodeChops.MagicEnums`.
2. Add a (global) using to namespace `CodeChops.MagicEnums`.
3. Create a record which implements one of the [enum types](#Enum-types).
4. Add members by creating `static readonly` members that call `CreateMember()`.


## API
| Method                | Description                                                                                                                         |
|-----------------------|-------------------------------------------------------------------------------------------------------------------------------------|
| `CreateMember`*       | Creates a new enum member and returns it.                                                                                           |
| `GetEnumerator`       | Gets an enumerator over the enum members.                                                                                           |
| `GetMembers`          | Gets an enumerable over:<br/>- All enum members, or<br/>- Members of a specific value: **Throws when no member has been found.**    |
| `GetValues`           | Gets an enumerable over the member values.                                                                                          |
| `TryGetMembers`       | Tries to get member(s) by value.                                                                                                    |
| `TryGetSingleMember`  | Tries to get a single member by name / value.<br/>**Throws when multiple members of the same value have been found.**               |
| `GetSingleMember`     | Gets a single member by name / value.<br/>**Throws when not found or multiple members have been found.**                            |
| `GetUniqueValueCount` | Gets the unique member value count.                                                                                                 |
| `GetMemberCount`      | Gets the member count.                                                                                                              |
| `GetDefaultValue`     | Gets the default value of the enum.                                                                                                 |
| `GetOrCreateMember`*  | Creates a member or gets one if a member already exists.                                                                            |

> The methods are `public`, except for the ones marked with *, they are `protected`.

# Enum types

## Number enums
Number enums (default) have a numeric type as value.
- Can be created by implementing `MagicEnum<TSelf, TNumber>`.
- If `TNumber` is omitted, `int` will be used as type: `MagicEnum<TSelf>`.
- `TNumber` can be of any type that are also supported by the default .NET implementation: `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, or `ulong`.
- Unlike the default C# .NET implementation, `decimal` is also supported.
- Implicit and explicit value declaration are supported, see the example below.

```csharp
/* This example shows an int enum with implicit and explicit values. */

public record StarRating : MagicEnum<StarRating>
{
    public static readonly StarRating One   = CreateMember(1);
    public static readonly StarRating Two   = CreateMember();    
    public static readonly StarRating Three = CreateMember();
    public static readonly StarRating Four  = CreateMember();
    public static readonly StarRating Five  = CreateMember();
}
```

The example creates an enum with an `int` as member value.
The value of the first member is explicitly defined.
Other values are being incremented automatically, because they are defined implicitly.

### Example
```csharp
/* This example shows the usage of a `decimal` as member value. */

public record EurUsdRate : MagicEnum<EurUsdRate, decimal>
{
    public static readonly EurUsdRate Average2021 = CreateMember(0.846m);
    public static readonly EurUsdRate Average2020 = CreateMember(0.877m);
    public static readonly EurUsdRate Average2019 = CreateMember(0.893m);
    public static readonly EurUsdRate Average2018 = CreateMember(0.848m);
}
```

## Flags enums
Flag enums are supported just like in the default .NET implementation. Implicit value declaration is also support. See the example below.
Flags enums offer extra methods:
- `GetUniqueFlags()`: Gets the unique flags of the provided value.
- `HasFlag()`: Returns `true` if a specific enum member contains the provided flag.

### Example
```csharp
/* This example shows the usage of a flags enum. 
   Note that member 'ReadAndWrite' flags both 'Read' and 'Write'. */

public record Permission : MagicFlagsEnum<Permission>
{
    public static readonly Permission None          = CreateMember(); // 0
    public static readonly Permission Read          = CreateMember(); // 1 << 0 
    public static readonly Permission Write         = CreateMember(); // 1 << 1
    public static readonly Permission ReadAndWrite  = CreateMember(Read | Write);
}
```

## String enums
Sometimes you only need an enumeration of `strings` (for example: names). In this case the underlying numeric value is not important. Magic string enums helps you achieving this:
- Can be created by implementing `MagicStringEnum<TSelf>`.
- Ensure that the values of the members are equal to the name of the members. This can be manually overriden, if necessary.
- Prohibit incorrect usage of numeric values when they are not needed.
- Remove the need to keep track of (incremental) numeric values.
- When members are generated, they show an automatic warning in the comments that the members shouldn't be renamed. See [member generation](#Member-generation).

### Example

```csharp
/* This example shows the creation of a string enum. 
   The value of the members are equal to the name of the members. */

using CodeChops.MagicEnums;

public record ErrorCode : MagicStringEnum<ErrorCode>
{
    public static readonly ErrorCode EndpointDoesNotExist   = CreateMember();
    public static readonly ErrorCode InvalidParameters      = CreateMember();    
    public static readonly ErrorCode NotAuthorized          = CreateMember();
}
```

## Custom enums
Custom enums can also be created. They offer a way to create an enum of any type that you prefer:
- Can be created by implementing `MagicCustomEnum<TSelf, TValue>`.
- `TValue` should implement `IEquatable` and `IComparable`.
- A custom value type can easily be generated using the [Value object generator](https://github.com/Code-Chops/DomainModeling/#Value-Object-Generator) which is included in the [Domain Modeling-library](https://github.com/Code-Chops/DomainModeling/).
- Two dogfooding examples of the usage custom enums:
  - See [Strict direction modes](https://github.com/Code-Chops/Geometry/#Strict-direction-modes) in the [Geometry library](https://github.com/Code-Chops/Geometry). It contains enums that have a `2D-point` as member value.
  - See [Implementation discovery](https://github.com/Code-Chops/ImplementationDiscovery), which automatically creates an enum of every implementation of a specific `base class` / `interface`. Each member contains its (uninitialized) instance as value.

# Pattern matching
To achieve pattern matching, you can do the following below.
```csharp
var message = level.Name switch
{
    nameof(Level.Low)    => "The level is low.", 
    nameof(Level.Medium) => "The level is medium.",
    nameof(Level.High)   => "The level is high.",
    _                    => throw new UnreachableException($"This should not occur.")
};
```
> In this example, the enum from the [default usage example](#Simple-enum) is used.

Another way is to define the types in an inner class and use them as the type of an enum member:
```csharp
var speedingFineInEur = vehicle switch
{
    Vehicle.Type.MotorCycle => 60,
    Vehicle.Type.Car        => 100,
    _                       => 0,
};
```
> In this example, the enum from the [complex usage example](#Complex-enum) is used.

# Optimization
Generally your enum does not dynamically add members at runtime. If this is the case, the attribute `DisableConcurrency` can be placed on the enum. It disables concurrency and therefore optimises memory usage and speed.

> Warning! Only use this attribute when you are sure that no race conditions can take place when creating / reading members.