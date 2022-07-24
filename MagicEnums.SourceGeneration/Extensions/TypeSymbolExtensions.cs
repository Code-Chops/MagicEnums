using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace CodeChops.MagicEnums.SourceGeneration.Extensions;

/// <summary>
/// Provides extensions on <see cref="ITypeSymbol"/>.
/// </summary>
internal static class TypeSymbolExtensions
{
	private static IReadOnlyCollection<string> ConversionOperatorNames { get; } = new[]
	{
		"op_Implicit", "op_Explicit",
	};

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is of type <typeparamref name="T"/>.
	/// </summary>
	public static bool IsType<T>(this ITypeSymbol typeSymbol)
	{
		return typeSymbol.IsType(typeof(T));
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is of the given type.
	/// </summary>
	public static bool IsType(this ITypeSymbol typeSymbol, Type type)
	{
		if (type.Namespace is null) return false;
		
		if (!IsType(typeSymbol, type.Name, type.Namespace)) return false;

		if (!typeSymbol.IsType(type.Name, type.Namespace!)) return false;

		return !type.IsGenericType || HasGenericTypeArguments(typeSymbol, type);


		// Local function that returns whether the input types have matching generic type arguments
		static bool HasGenericTypeArguments(ITypeSymbol typeSymbol, Type type)
		{
			if (typeSymbol is not INamedTypeSymbol namedTypeSymbol) return false;

			var requiredTypeArgs = type.GenericTypeArguments;
			var actualTypeArgs = namedTypeSymbol.TypeArguments;

			if (requiredTypeArgs.Length != actualTypeArgs.Length) return false;

			for (var i = 0; i < requiredTypeArgs.Length; i++)
				if (!actualTypeArgs[i].IsType(requiredTypeArgs[i]))
					return false;

			return true;
		}
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> has the given <paramref name="fullTypeName"/>.
	/// </summary>
	/// <param name="fullTypeName">The type name including the namespace, e.g. System.Object.</param>
	public static bool IsType(this ITypeSymbol typeSymbol, string fullTypeName, bool? isGenericType = null)
	{
		var fullTypeNameSpan = fullTypeName.AsSpan();

		var lastDotIndex = fullTypeNameSpan.LastIndexOf('.');

		if (lastDotIndex < 1) return false;

		var typeName = fullTypeNameSpan.Slice(1 + lastDotIndex);
		var containingNamespace = fullTypeNameSpan.Slice(0, lastDotIndex);

		return IsType(typeSymbol, typeName, containingNamespace, isGenericType);
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> has the given <paramref name="typeName"/> and <paramref name="containingNamespace"/>.
	/// </summary>
	public static bool IsType(this ITypeSymbol typeSymbol, string typeName, string containingNamespace, bool? isGenericType = null)
	{
		return IsType(typeSymbol, typeName.AsSpan(), containingNamespace.AsSpan(), isGenericType);
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> has the given <paramref name="typeName"/> and <paramref name="containingNamespace"/>.
	/// </summary>
	/// <param name="isGenericType">If not null, the being-generic of the type must match this value.</param>
	private static bool IsType(this ITypeSymbol typeSymbol, ReadOnlySpan<char> typeName, ReadOnlySpan<char> containingNamespace, bool? isGenericType = null)
	{
		var result = typeSymbol.Name.AsSpan().Equals(typeName, StringComparison.Ordinal) &&
			typeSymbol.ContainingNamespace.HasFullName(containingNamespace);

		if (result && isGenericType is not null)
			result = typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType == isGenericType.Value;

		return result;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is or inherits from a certain class, as determined by the given <paramref name="predicate"/>.
	/// </summary>
	public static bool IsOrInheritsClass(this ITypeSymbol typeSymbol, Func<ITypeSymbol, bool> predicate, out ITypeSymbol targetType)
	{
		var baseType = (ITypeSymbol?)typeSymbol;
		while ((baseType = baseType!.BaseType) is not null)
		{
			// End of inheritance chain
			if (baseType.IsType<object>())
				break;

			if (predicate(baseType))
			{
				targetType = baseType;
				return true;
			}
		}

		targetType = null!;
		return false;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is or implements a certain interface, as determined by the given <paramref name="predicate"/>.
	/// </summary>
	public static bool IsOrImplementsInterface(this ITypeSymbol typeSymbol, Func<ITypeSymbol, bool> predicate, out ITypeSymbol targetType)
	{
		if (predicate(typeSymbol))
		{
			targetType = typeSymbol;
			return true;
		}

		foreach (var interf in typeSymbol.AllInterfaces)
		{
			if (predicate(interf))
			{
				targetType = interf;
				return true;
			}
		}

		targetType = null!;
		return false;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is a constructed generic type with a single type argument matching the <paramref name="requiredTypeArgument"/>.
	/// </summary>
	public static bool HasSingleGenericTypeArgument(this ITypeSymbol typeSymbol, ITypeSymbol requiredTypeArgument)
	{
		return typeSymbol is INamedTypeSymbol namedTypeSymbol &&
			namedTypeSymbol.TypeArguments.Length == 1 &&
			namedTypeSymbol.TypeArguments[0].Equals(requiredTypeArgument, SymbolEqualityComparer.Default);
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> represents an integral type, such as <see cref="int"/> or <see cref="ulong"/>.
	/// </summary>
	/// <param name="seeThroughNullable">Whether to return true for a <see cref="Nullable{T}"/> of a matching underlying type.</param>
	/// <param name="includeDecimal">Whether to consider <see cref="decimal"/> as an integral type.</param>
	public static bool IsIntegral(this ITypeSymbol typeSymbol, bool seeThroughNullable, bool includeDecimal = false)
	{
		if (typeSymbol.IsNullable(out var underlyingType) && seeThroughNullable)
			typeSymbol = underlyingType;

		var result = typeSymbol.IsType<byte>()
			|| typeSymbol.IsType<sbyte>()
			|| typeSymbol.IsType<ushort>()
			|| typeSymbol.IsType<short>()
			|| typeSymbol.IsType<uint>()
			|| typeSymbol.IsType<int>()
			|| typeSymbol.IsType<ulong>()
			|| typeSymbol.IsType<long>()
			|| (includeDecimal && typeSymbol.IsType<decimal>());

		return result;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is a generic type with the given number of type parameters.
	/// </summary>
	public static bool IsGeneric(this ITypeSymbol typeSymbol, int typeParameterCount)
	{
		if (typeSymbol is not INamedTypeSymbol namedTypeSymbol) return false;

		var result = namedTypeSymbol.IsGenericType && namedTypeSymbol.TypeParameters.Length == typeParameterCount;
		return result;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is a generic type with the given number of type parameters.
	/// Outputs the type arguments on true.
	/// </summary>
	public static bool IsGeneric(this ITypeSymbol typeSymbol, int typeParameterCount, out ImmutableArray<ITypeSymbol> typeArguments)
	{
		typeArguments = default;

		if (typeSymbol is not INamedTypeSymbol namedTypeSymbol) return false;

		if (!IsGeneric(typeSymbol, typeParameterCount)) return false;

		typeArguments = namedTypeSymbol.TypeArguments;
		return true;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is a <see cref="Nullable{T}"/>.
	/// </summary>
	public static bool IsNullable(this ITypeSymbol typeSymbol)
	{
		return typeSymbol.IsNullable(out _);
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is a <see cref="Nullable{T}"/>, outputting the underlying type if so.
	/// </summary>
	public static bool IsNullable(this ITypeSymbol typeSymbol, out ITypeSymbol underlyingType)
	{
		if (typeSymbol is INamedTypeSymbol namedTypeSymbol && typeSymbol.IsType("System.Nullable", isGenericType: true))
		{
			underlyingType = namedTypeSymbol.TypeArguments[0];
			return true;
		}

		underlyingType = null!;
		return false;
	}

	/// <summary>
	/// Returns whether the given <see cref="ITypeSymbol"/> implements <see cref="IEquatable{T}"/> against itself.
	/// </summary>
	public static bool IsSelfEquatable(this ITypeSymbol typeSymbol)
	{
		return typeSymbol.IsOrImplementsInterface(interf => interf.IsType("IEquatable", "System", isGenericType: true) && interf.HasSingleGenericTypeArgument(typeSymbol), out _);
	}

	/// <summary>
	/// <para>
	/// Returns whether the <see cref="ITypeSymbol"/> implements any <see cref="IComparable"/> or <see cref="IComparable{T}"/> interface.
	/// </para>
	/// <para>
	/// This method can optionally see through <see cref="Nullable{T}"/> (which does not implement the necessary interface) to the underlying type.
	/// Beware that nullables <em>cannot</em> simply be compared with left.CompareTo(right).
	/// </para>
	/// </summary>
	/// <param name="seeThroughNullable">Whether to return true for a <see cref="Nullable{T}"/> of a matching underlying type.</param>
	public static bool IsComparable(this ITypeSymbol typeSymbol, bool seeThroughNullable)
	{
		if (seeThroughNullable && typeSymbol.IsNullable(out var underlyingType))
			typeSymbol = underlyingType;

		var result = typeSymbol.AllInterfaces.Any(interf => interf.IsType("System.IComparable"));
		return result;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is or implements <see cref="System.Collections.IEnumerable"/>.
	/// If so, this method outputs the element type of the most <em>concrete</em> <see cref="IEnumerable{T}"/> type it implements, if any.
	/// </summary>
	public static bool IsEnumerable(this ITypeSymbol typeSymbol, out INamedTypeSymbol? elementType)
	{
		elementType = null;

		if (!typeSymbol.IsOrImplementsInterface(type => type.IsType("IEnumerable", "System.Collections", isGenericType: false), out _))
			return false;

		if (typeSymbol.IsOrImplementsInterface(type => type.IsType("IList", "System.Collections.Generic", isGenericType: true), out var interf))
		{
			elementType = ((INamedTypeSymbol)interf).TypeArguments[0] as INamedTypeSymbol;
			return true;
		}
		if (typeSymbol.IsOrImplementsInterface(type => type.IsType("IReadOnlyList", "System.Collections.Generic", isGenericType: true), out interf))
		{
			elementType = ((INamedTypeSymbol)interf).TypeArguments[0] as INamedTypeSymbol;
			return true;
		}
		if (typeSymbol.IsOrImplementsInterface(type => type.IsType("ISet", "System.Collections.Generic", isGenericType: true), out interf))
		{
			elementType = ((INamedTypeSymbol)interf).TypeArguments[0] as INamedTypeSymbol;
			return true;
		}
		if (typeSymbol.IsOrImplementsInterface(type => type.IsType("IReadOnlySet", "System.Collections.Generic", isGenericType: true), out interf))
		{
			elementType = ((INamedTypeSymbol)interf).TypeArguments[0] as INamedTypeSymbol;
			return true;
		}
		if (typeSymbol.IsOrImplementsInterface(type => type.IsType("ICollection", "System.Collections.Generic", isGenericType: true), out interf))
		{
			elementType = ((INamedTypeSymbol)interf).TypeArguments[0] as INamedTypeSymbol;
			return true;
		}
		if (typeSymbol.IsOrImplementsInterface(type => type.IsType("IReadOnlyCollection", "System.Collections.Generic", isGenericType: true), out interf))
		{
			elementType = ((INamedTypeSymbol)interf).TypeArguments[0] as INamedTypeSymbol;
			return true;
		}
		if (typeSymbol.IsOrImplementsInterface(type => type.IsType("IEnumerable", "System.Collections.Generic", isGenericType: true), out interf))
		{
			elementType = ((INamedTypeSymbol)interf).TypeArguments[0] as INamedTypeSymbol;
			return true;
		}

		return true;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> or a base type has an override of <see cref="Object.Equals(object)"/> more specific than <see cref="Object"/>'s implementation.
	/// </summary>
	public static bool HasEqualsOverride(this ITypeSymbol typeSymbol, bool falseForStructs = false)
	{
		// Technically this could match an overridden "new" Equals defined by a base type, but that is a nonsense scenario
		var result = typeSymbol.GetMembers(nameof(Equals)).OfType<IMethodSymbol>().Any(method => method.IsOverride && !method.IsStatic &&
			method.Arity == 0 && method.Parameters.Length == 1 && method.Parameters[0].Type.IsType<object>());

		return result;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is annotated with the specified attribute.
	/// </summary>
	public static bool HasAttribute<TAttribute>(this ITypeSymbol typeSymbol, out AttributeData? attribute)
	{
		var result = typeSymbol.HasAttribute(attribute => attribute.IsType<TAttribute>(), out attribute);
		return result;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is annotated with the specified attribute.
	/// </summary>
	public static bool HasAttribute(this ITypeSymbol typeSymbol, string typeName, string containingNamespace, out AttributeData? attribute)
	{
		var alternativeTypeName = typeName.EndsWith("Attribute")
			? typeName.Substring(0, typeName.Length - "Attribute".Length)
			: $"{typeName}Attribute";

		var result = typeSymbol.HasAttribute(attribute => attribute.IsType(typeName, containingNamespace), out attribute)
			|| typeSymbol.HasAttribute(attribute => attribute.IsType(alternativeTypeName, containingNamespace), out attribute);

		return result;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is annotated with the specified attribute.
	/// </summary>
	public static bool HasAttribute(this ITypeSymbol typeSymbol, Func<INamedTypeSymbol, bool> predicate, out AttributeData? attribute)
	{
		attribute = typeSymbol.GetAttributes().FirstOrDefault(attribute => attribute.AttributeClass is not null && predicate(attribute.AttributeClass));
		return attribute is not null;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is annotated with the specified attribute.
	/// </summary>
	public static bool HasAttributes<TAttribute>(this ITypeSymbol typeSymbol, out IEnumerable<AttributeData> attributes)
	{
		var result = typeSymbol.HasAttributes(attribute => attribute.IsType<TAttribute>(), out attributes);
		return result;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is annotated with the specified attribute.
	/// </summary>
	public static bool HasAttributes(this ITypeSymbol typeSymbol, string typeName, string containingNamespace, out IEnumerable<AttributeData> attributes)
	{
		var alternativeTypeName = typeName.EndsWith("Attribute")
			? typeName.Substring(0, typeName.Length - "Attribute".Length)
			: $"{typeName}Attribute";

		var result = typeSymbol.HasAttributes(attribute => attribute.IsType(typeName, containingNamespace), out attributes)
			|| typeSymbol.HasAttributes(attribute => attribute.IsType(alternativeTypeName, containingNamespace), out attributes);

		return result;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> is annotated with the specified attribute.
	/// </summary>
	public static bool HasAttributes(this ITypeSymbol typeSymbol, Func<INamedTypeSymbol, bool> predicate, out IEnumerable<AttributeData> attributes)
	{
		attributes = typeSymbol.GetAttributes().Where(attribute => attribute.AttributeClass is not null && predicate(attribute.AttributeClass));
		return attributes.Any();
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> defines a conversion to the specified type.
	/// </summary>
	public static bool HasConversionTo(this ITypeSymbol typeSymbol, string typeName, string containingNamespace)
	{
		var result = !typeSymbol.IsType(typeName, containingNamespace) && typeSymbol.GetMembers().Any(member =>
			member is IMethodSymbol method && ConversionOperatorNames.Contains(method.Name) && member.DeclaredAccessibility == Accessibility.Public &&
			method.ReturnType.IsType(typeName, containingNamespace));
		return result;
	}

	/// <summary>
	/// Returns whether the <see cref="ITypeSymbol"/> defines a conversion from the specified type.
	/// </summary>
	public static bool HasConversionFrom(this ITypeSymbol typeSymbol, string typeName, string containingNamespace)
	{
		var result = !typeSymbol.IsType(typeName, containingNamespace) && typeSymbol.GetMembers().Any(member =>
			member is IMethodSymbol method && ConversionOperatorNames.Contains(method.Name) && member.DeclaredAccessibility == Accessibility.Public &&
			method.Parameters.Length == 1 && method.Parameters[0].Type.IsType(typeName, containingNamespace));
		return result;
	}

	/// <summary>
	/// Enumerates the primitive types (string, int, bool, etc.) from which the given <see cref="ITypeSymbol"/> is convertible.
	/// </summary>
	/// <param name="skipForSystemTypes">If true, if the given type is directly under the System namespace, this method yields nothing.</param>
	public static IEnumerable<Type> GetAvailableConversionsFromPrimitives(this ITypeSymbol typeSymbol, bool skipForSystemTypes)
	{
		if (skipForSystemTypes && typeSymbol.ContainingNamespace.Name == "System" && (typeSymbol.ContainingNamespace.ContainingNamespace?.IsGlobalNamespace ?? true))
			yield break;

		if (typeSymbol.HasConversionFrom("Boolean", "System")) yield return typeof(bool);

		if (typeSymbol.HasConversionFrom("Byte", "System")) yield return typeof(byte);
		if (typeSymbol.HasConversionFrom("SByte", "System")) yield return typeof(sbyte);
		if (typeSymbol.HasConversionFrom("UInt16", "System")) yield return typeof(ushort);
		if (typeSymbol.HasConversionFrom("Int16", "System")) yield return typeof(short);
		if (typeSymbol.HasConversionFrom("UInt32", "System")) yield return typeof(uint);
		if (typeSymbol.HasConversionFrom("Int32", "System")) yield return typeof(int);
		if (typeSymbol.HasConversionFrom("UInt64", "System")) yield return typeof(ulong);
		if (typeSymbol.HasConversionFrom("Int64", "System")) yield return typeof(long);
	}

	/// <summary>
	/// Returns the code for a string expression of the given <paramref name="memberName"/> of "this".
	/// </summary>
	/// <param name="memberName">The member name. For example, "Value" leads to a string of "this.Value".</param>
	/// <param name="stringVariant">The expression to use for strings. Any {0} is replaced by the member name.</param>
	public static string CreateStringExpression(this ITypeSymbol typeSymbol, string memberName, string stringVariant = "this.{0}")
	{
		if (typeSymbol.IsValueType && !typeSymbol.IsNullable()) return $"this.{memberName}.ToString()";
		if (typeSymbol.IsType<string>()) return String.Format(stringVariant, memberName);
		return $"this.{memberName}?.ToString()";
	}

	/// <summary>
	/// Returns the code for a comparison expression on the given <paramref name="memberName"/> between "this" and "other".
	/// </summary>
	/// <param name="memberName">The member name. For example, "Value" leads to a comparison between "this.Value" and "other.Value".</param>
	/// <param name="stringVariant">The expression to use for strings. Any {0} is replaced by the member name.</param>
	public static string CreateComparisonExpression(this ITypeSymbol typeSymbol, string memberName, string stringVariant = "String.Compare(this.{0}, other.{0}, StringComparison.Ordinal)")
	{
		// DO NOT REORDER

		// Collections have not been implemented, as we do not generate CompareTo() if any data member is not IComparable (as is the case for collections)

		if (typeSymbol.IsType<string>()) return String.Format(stringVariant, memberName);
		if (typeSymbol.IsNullable()) return $"(this.{memberName} is null || other.{memberName} is null ? -(this.{memberName} is null).CompareTo(other.{memberName} is null) : this.{memberName}.Value.CompareTo(other.{memberName}.Value))";
		if (typeSymbol.IsValueType) return $"this.{memberName}.CompareTo(other.{memberName})";
		return $"(this.{memberName} is null || other.{memberName} is null ? -(this.{memberName} is null).CompareTo(other.{memberName} is null) : this.{memberName}.CompareTo(other.{memberName}))";
	}

	/// <summary>
	/// Converts names like 'string' to 'System.String'
	/// </summary>
	public static string GetTypeFullName(this ITypeSymbol typeSymbol) =>
		typeSymbol.SpecialType == SpecialType.None
			? typeSymbol.ToDisplayString()
			: typeSymbol.SpecialType.ToString().Replace("_", ".");
}