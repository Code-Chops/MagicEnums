namespace CodeChops.MagicEnums.UnitTests.CreationExamples;

public class StronglyTypedCreationTests
{
	public abstract record Vehicle(int WheelCount) : MagicEnum<Vehicle>
	{
		public static readonly Type.Bicycle	Bicycle		= CreateMember<Type.Bicycle>();
		public static readonly Type.Step	Step		= CreateMember<Type.Step>();
		public static readonly Type.Car		FuelCar		= CreateMember(() => new Type.Car(EmitsCo2: true));
		public static readonly Type.Car		ElectricCar	= CreateMember(() => new Type.Car(EmitsCo2: true));

		public record Type
		{
			public record Bicycle()				: Vehicle(WheelCount: 2);
			public record Car(bool EmitsCo2)	: Vehicle(WheelCount: 4);
			public record Step()				: Vehicle(WheelCount: 2);
		}
	}

	[Fact]
	public void Test()
	{
		Assert.Equal(1, Vehicle.Step.Value);
	}
}