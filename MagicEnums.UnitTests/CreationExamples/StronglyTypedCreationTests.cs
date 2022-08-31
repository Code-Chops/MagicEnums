// ReSharper disable MemberHidesStaticFromOuterClass
namespace CodeChops.MagicEnums.UnitTests.CreationExamples;

public class StronglyTypedCreationTests
{
	public record Vehicle(int WheelCount) : MagicEnum<Vehicle>
	{
		public static readonly Vehicle		Default		= CreateMember();
		public static readonly Type.Bicycle	Bicycle		= CreateMember<Type.Bicycle>();
		public static readonly Type.Scooter	Scooter		= GetOrCreateMember<Type.Scooter>(2);
		public static readonly Type.Car		FuelCar		= CreateMember(() => new Type.Car(EmitsCo2: true));
		public static readonly Type.Car		ElectricCar	= CreateMember(() => new Type.Car(EmitsCo2: false));

		public static class Type
		{
			public record Bicycle()				: Vehicle(WheelCount: 2);
			public record Car(bool EmitsCo2)	: Vehicle(WheelCount: 4);
			public record Scooter()				: Vehicle(WheelCount: 2);
		}
	}

	[Fact]
	public void Test()
	{
		Assert.IsType<Vehicle>(Vehicle.Default);
		Assert.IsType<Vehicle.Type.Bicycle>(Vehicle.Bicycle);
		Assert.Equal(2, Vehicle.Scooter.Value);
		Assert.False(Vehicle.ElectricCar.EmitsCo2);
		Assert.Equal(4, Vehicle.FuelCar.WheelCount);
	}
}