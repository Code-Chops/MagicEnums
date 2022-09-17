// ReSharper disable MemberHidesStaticFromOuterClass
namespace CodeChops.MagicEnums.UnitTests.CreationExamples;

public class StronglyTypedCreationTests
{
	public record Vehicle(int WheelCount) : MagicEnum<Vehicle>
	{
		public static readonly Vehicle			Default		= CreateMember();
		public static readonly Type.Bicycle		Bicycle		= CreateMember<Type.Bicycle>();
		public static readonly Type.MotorCycle	MotorCycle	= CreateMember<Type.MotorCycle>();
		public static readonly Type.Car			FuelCar		= CreateMember(() => new Type.Car(EmitsCo2: true));
		public static readonly Type.Car			ElectricCar	= CreateMember(() => new Type.Car(EmitsCo2: false));
		
		public static class Type
		{
			public record Bicycle()				: Vehicle(WheelCount: 2);
			public record MotorCycle()			: Vehicle(WheelCount: 2);
			public record Car(bool EmitsCo2)	: Vehicle(WheelCount: 4);
		} 
	}
	
	[Fact]
	public void Test()
	{
		Assert.IsType<Vehicle>(Vehicle.Default);
		Assert.IsType<Vehicle.Type.Bicycle>(Vehicle.Bicycle);
		Assert.Equal(2, Vehicle.MotorCycle.Value);
		Assert.False(Vehicle.ElectricCar.EmitsCo2);
		Assert.Equal(4, Vehicle.FuelCar.WheelCount);

		var vehicle = (Vehicle)Vehicle.Bicycle;

		// ReSharper disable once UnusedVariable
		var speedingFineInEur = vehicle switch
		{
			Vehicle.Type.MotorCycle => 60,
			Vehicle.Type.Car		=> 100,
			_						=> 0,
		};
	}
}