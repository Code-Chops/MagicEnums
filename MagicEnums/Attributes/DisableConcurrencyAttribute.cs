﻿namespace CodeChops.MagicEnums.Attributes;

/// <summary>
/// Add this attribute to the enum to disable concurrency and therefore optimise the memory usage and speed.
/// Warning! Only use this label when you are sure that no race conditions can take place when creating / reading members.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DisableConcurrencyAttribute : Attribute
{
}