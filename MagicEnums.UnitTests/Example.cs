// ReSharper disable MemberHidesStaticFromOuterClass
using Xunit.Abstractions;

namespace CodeChops.MagicEnums.UnitTests;

public class StronglyTypedCreationTests
{
	private ITestOutputHelper TestOutputHelper { get; }

	public StronglyTypedCreationTests(ITestOutputHelper testOutputHelper)
	{
		this.TestOutputHelper = testOutputHelper;
	}

	public record Vehicle(int WheelCount) : MagicEnum<Vehicle>
	{
		public static readonly Type.Bicycle		Bicycle		= CreateMember<Type.Bicycle>();
		public static readonly Type.MotorCycle	MotorCycle	= CreateMember<Type.MotorCycle>();
		public static readonly Type.Car			FuelCar		= CreateMember<Type.Car>(() => new(EmitsCo2: true));
		public static readonly Type.Car			ElectricCar	= CreateMember<Type.Car>(() => new(EmitsCo2: false));
		
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
		Assert.IsType<Vehicle.Type.Bicycle>(Vehicle.Bicycle);
		Assert.Equal(1, Vehicle.MotorCycle.Value);
		Assert.False(Vehicle.ElectricCar.EmitsCo2);
		Assert.Equal(4, Vehicle.FuelCar.WheelCount);

		var vehicle = (Vehicle)Vehicle.Bicycle;

		var speedingFineInEur = vehicle switch
		{
			Vehicle.Type.MotorCycle => 60,
			Vehicle.Type.Car		=> 100,
			_						=> 0,
		};

		this.TestOutputHelper.WriteLine(speedingFineInEur.ToString());
	}
}