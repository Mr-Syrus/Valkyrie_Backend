using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using valkyrie.Models.Cars;
using valkyrie.Models.Companies;

namespace valkyrie.Models.Events
{
	public class Event
	{
		// id (PK) : INTEGER
		[Key]
		public int Id { get; set; }

		// platform_id (FK) : INTEGER
		[Required]
		public int PlatformId { get; set; }

		[ForeignKey(nameof(PlatformId))]
		public Platform Platforms { get; set; } = null!;

		// car_id (FK) : INTEGER
		[Required]
		public int CarId { get; set; }

		[ForeignKey(nameof(CarId))]
		public Car Car { get; set; } = null!;

		// date_time : TIMESTAMP WITH TIME ZONE
		[Required]
		public DateTimeOffset DateTime { get; set; }

		// type_event_id (FK) : INTEGER
		[Required]
		public int TypeEventId { get; set; }

		[ForeignKey(nameof(TypeEventId))]
		public TypeEvent TypeEvent { get; set; } = null!;

		// engine_torque : REAL
		public double? EngineTorque { get; set; }

		// engine_load : REAL
		public double? EngineLoad { get; set; }

		// engine_oil_pressure : REAL
		public double? EngineOilPressure { get; set; }

		// engine_il_temperature : REAL
		public double? EngineILTemperature { get; set; }

		// exhaust_gas_temperature : REAL
		public double? ExhaustGasTemperature { get; set; }

		// engine_operating_hours : INTERVAL
		public TimeSpan? EngineOperatingHours { get; set; }

		// transmission_temperature : REAL
		public double? TransmissionTemperature { get; set; }

		// remaining_fuel : REAL
		public double? RemainingFuel { get; set; }

		// remaining_fuel_real_time : REAL
		public double? RemainingFuelRealTime { get; set; }

		// pressure_hydraulic_system : REAL
		public double? PressureHydraulicSystem { get; set; }

		// hydraulic_fuid_temperature : REAL
		public double? HydraulicFluidTemperature { get; set; }

		// battery_voltage : REAL
		public double? BatteryVoltage { get; set; }

		// latitude : NUMERIC(9,6)
		[Required]
		public decimal Latitude { get; set; }

		// longitude : NUMERIC(9,6)
		[Required]
		public decimal Longitude { get; set; }
	}
}
