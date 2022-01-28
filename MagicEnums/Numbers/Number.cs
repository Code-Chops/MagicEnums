namespace CodeChops.MagicEnums.Numbers;

/// <summary>
/// From: https://codereview.stackexchange.com/questions/26022/generic-calculator-and-generic-number
/// </summary>
/// <typeparam name="T">Integral type</typeparam>
public readonly record struct Number<T>
	where T : struct, IComparable<T>, IEquatable<T>, IConvertible
{
	public static Number<T> Empty { get; } = new();
	public override string ToString() => $"{this.Value:F2}";

	private T Value { get; }

	public Number(T value)
	{
		this.Value = value;
	}

	public static bool operator <(Number<T> a, Number<T> b)
	{
		return a.Value.CompareTo(b.Value) < 0;
	}

	public static bool operator <=(Number<T> a, Number<T> b)
	{
		return a.Value.CompareTo(b.Value) <= 0;
	}

	public static bool operator >(Number<T> a, Number<T> b)
	{
		return a.Value.CompareTo(b.Value) > 0;
	}

	public static bool operator >=(Number<T> a, Number<T> b)
	{
		return a.Value.CompareTo(b.Value) >= 0;
	}

	public static Number<T> operator !(Number<T> a)
	{
		return new(Calculator<T>.Negate(a.Value));
	}

	public static Number<T> operator +(Number<T> a, Number<T> b)
	{
		return new(Calculator<T>.Add(a.Value, b.Value));
	}

	public static Number<T> operator -(Number<T> a, Number<T> b)
	{
		return new(Calculator<T>.Subtract(a.Value, b.Value));
	}

	public static Number<T> operator *(Number<T> a, Number<T> b)
	{
		return new(Calculator<T>.Multiply(a.Value, b.Value));
	}

	public static Number<T> operator /(Number<T> a, Number<T> b)
	{
		return new(Calculator<T>.Divide(a.Value, b.Value));
	}

	public static Number<T> operator %(Number<T> a, Number<T> b)
	{
		return new(Calculator<T>.Modulo(a.Value, b.Value));
	}

	public static Number<T> operator -(Number<T> a)
	{
		return new(Calculator<T>.Negate(a.Value));
	}

	public static Number<T> operator +(Number<T> a)
	{
		return new(Calculator<T>.Plus(a.Value));
	}

	public static Number<T> operator ++(Number<T> a)
	{
		var number = new Number<T>(Calculator<T>.Increment(a.Value));
		return number;
	}

	public static Number<T> operator --(Number<T> a)
	{
		return new(Calculator<T>.Decrement(a.Value));
	}

	public static implicit operator Number<T>(T value)
	{
		return new(value);
	}

	public static implicit operator T(Number<T> value)
	{
		return value.Value;
	}

	public static Number<T> Create<TSourceNumber>(TSourceNumber number)
	{
		if (number == null) throw new ArgumentNullException(nameof(number));

		return new((T)Convert.ChangeType(number, typeof(T)));
	}
}

public static class NumberExtensions
{
	public static Number<TNumber> Cast<TSourceNumber, TNumber>(this Number<TSourceNumber> number)
		where TNumber : struct, IComparable<TNumber>, IEquatable<TNumber>, IConvertible
		where TSourceNumber : struct, IComparable<TSourceNumber>, IEquatable<TSourceNumber>, IConvertible
	{
		return new((TNumber)Convert.ChangeType((TSourceNumber)number, typeof(TNumber)));
	}
}