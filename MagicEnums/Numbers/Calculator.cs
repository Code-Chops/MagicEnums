using System.Linq.Expressions;

namespace CodeChops.MagicEnums.Numbers;

/// <summary>
/// Class to allow operations (like Add, Multiply, etc.) for generic types. This type should allow these operations themselves.
/// If a type does not support an operation, an exception is throw when using this operation, not during construction of this class.
/// From: https://codereview.stackexchange.com/questions/26022/generic-calculator-and-generic-number
/// </summary>
internal sealed class Calculator<T>
{
	static Calculator()
	{
		Add = CreateDelegate<T>(Expression.AddChecked, "Addition", isChecked: true);
		Subtract = CreateDelegate<T>(Expression.SubtractChecked, "Substraction", isChecked: true);
		Multiply = CreateDelegate<T>(Expression.MultiplyChecked, "Multiply", isChecked: true);
		Divide = CreateDelegate<T>(Expression.Divide, "Divide", isChecked: true);
		Modulo = CreateDelegate<T>(Expression.Modulo, "Modulus", isChecked: true);
		Negate = CreateDelegate(Expression.NegateChecked, "Negate", isChecked: true);
		Plus = CreateDelegate(Expression.UnaryPlus, "Plus", isChecked: true);
		Increment = CreateDelegate(Expression.Increment, "Increment", isChecked: true);
		Decrement = CreateDelegate(Expression.Decrement, "Decrement", isChecked: true);
	}

	/// <summary>
	/// Adds two values of the same type.
	/// Supported by: All numeric values.
	/// </summary>
	/// <exception cref="OverflowException"/>
	/// <exception cref="InvalidOperationException"/>
	public static Func<T, T, T> Add { get; }

	/// <summary>
	/// Subtracts two values of the same type.
	/// Supported by: All numeric values.
	/// </summary>
	/// <exception cref="OverflowException"/>
	/// <exception cref="InvalidOperationException"/>
	public static Func<T, T, T> Subtract { get; }

	/// <summary>
	/// Multiplies two values of the same type.
	/// Supported by: All numeric values.
	/// </summary>
	/// <exception cref="OverflowException"/>
	/// <exception cref="InvalidOperationException"/>
	public static Func<T, T, T> Multiply { get; }

	/// <summary>
	/// Divides two values of the same type.
	/// Supported by: All numeric values.
	/// </summary>
	/// <exception cref="OverflowException"/>
	/// <exception cref="InvalidOperationException"/>
	public static Func<T, T, T> Divide { get; }

	/// <summary>
	/// Divides two values of the same type and returns the remainder.
	/// Supported by: All numeric values.
	/// </summary>
	/// <exception cref="OverflowException"/>
	/// <exception cref="InvalidOperationException"/>
	public static Func<T, T, T> Modulo { get; }

	/// <summary>
	/// Gets the negative value of T.
	/// Supported by: All numeric values, but will throw an OverflowException on unsigned values which are not 0.
	/// </summary>
	/// <exception cref="OverflowException"/>
	/// <exception cref="InvalidOperationException"/>
	public static Func<T, T> Negate { get; }

	/// <summary>
	/// Gets the negative value of T.
	/// Supported by: All numeric values.
	/// </summary>
	/// <exception cref="InvalidOperationException"/>
	public static Func<T, T> Plus { get; }

	/// <summary>
	/// Gets the negative value of T.
	/// Supported by: All numeric values.
	/// </summary>
	/// <exception cref="OverflowException"/>
	/// <exception cref="InvalidOperationException"/>
	public static Func<T, T> Increment { get; }

	/// <summary>
	/// Gets the negative value of T.
	/// Supported by: All numeric values.
	/// </summary>
	/// <exception cref="OverflowException"/>
	/// <exception cref="InvalidOperationException"/>
	public static Func<T, T> Decrement { get; }

	private static Func<T, T2, T> CreateDelegate<T2>(Func<Expression, Expression, Expression> @operator, string operatorName, bool isChecked)
	{
		try
		{
			var convertToTypeA = ConvertTo(typeof(T));
			var convertToTypeB = ConvertTo(typeof(T2));
			var parameterA = Expression.Parameter(typeof(T), "a");
			var parameterB = Expression.Parameter(typeof(T2), "b");
			var valueA = (convertToTypeA != null) ? Expression.Convert(parameterA, convertToTypeA) : (Expression)parameterA;
			var valueB = (convertToTypeB != null) ? Expression.Convert(parameterB, convertToTypeB) : (Expression)parameterB;
			var body = @operator(valueA, valueB);

			if (convertToTypeA != null)
			{
				body = isChecked ? Expression.ConvertChecked(body, typeof(T)) : Expression.Convert(body, typeof(T));
			}

			return Expression.Lambda<Func<T, T2, T>>(body, parameterA, parameterB).Compile();
		}
		catch
		{
			return (a, b) =>
			{
				throw new InvalidOperationException("Operator " + operatorName + " is not supported by type " + typeof(T).FullName + ".");
			};
		}
	}

	private static Func<T, T> CreateDelegate(Func<Expression, Expression> @operator, string operatorName, bool isChecked)
	{
		try
		{
			var convertToType = ConvertTo(typeof(T));
			var parameter = Expression.Parameter(typeof(T), "a");
			var value = (convertToType != null) ? Expression.Convert(parameter, convertToType) : (Expression)parameter;
			var body = @operator(value);

			if (convertToType != null)
			{
				body = isChecked ? Expression.ConvertChecked(body, typeof(T)) : Expression.Convert(body, typeof(T));
			}

			return Expression.Lambda<Func<T, T>>(body, parameter).Compile();
		}
		catch
		{
			return a =>
			{
				throw new InvalidOperationException("Operator " + operatorName + " is not supported by type " + typeof(T).FullName + ".");
			};
		}
	}

	public static Type? ConvertTo(Type type)
	{
		return Type.GetTypeCode(type) is TypeCode.Char or TypeCode.Byte or TypeCode.SByte or TypeCode.Int16 or TypeCode.UInt16
			? typeof(int)
			: null;
	}
}