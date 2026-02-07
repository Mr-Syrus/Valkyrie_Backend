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
    [Column("id")]
    public int Id { get; set; }

    // platform_id (FK) : INTEGER
    [Required]
    [Column("platform_id")]
    public int PlatformId { get; set; }

    [ForeignKey(nameof(PlatformId))]
    public Platform Platforms { get; set; } = null!;

    // car_id (FK) : INTEGER
    [Required]
    [Column("car_id")]
    public int CarId { get; set; }

    [ForeignKey(nameof(CarId))]
    public Car Car { get; set; } = null!;

    // date_time : TIMESTAMP WITH TIME ZONE
    [Required]
    [Column("date_time")]
    public DateTimeOffset DateTime { get; set; }

    // type_event_id (FK) : INTEGER
    [Required]
    [Column("type_event_id")]
    public int TypeEventId { get; set; }

    [ForeignKey(nameof(TypeEventId))]
    public TypeEvent TypeEvent { get; set; } = null!;

    // engine_torque : REAL
    [Column("engine_torque")]
    public double? EngineTorque { get; set; }

    // engine_load : REAL
    [Column("engine_load")]
    public double? EngineLoad { get; set; }

    // engine_oil_pressure : REAL
    [Column("engine_oil_pressure")]
    public double? EngineOilPressure { get; set; }

    // engine_il_temperature : REAL
    [Column("engine_il_temperature")]
    public double? EngineILTemperature { get; set; }

    // exhaust_gas_temperature : REAL
    [Column("exhaust_gas_temperature")]
    public double? ExhaustGasTemperature { get; set; }

    // engine_operating_hours : INTERVAL
    [Column("engine_operating_hours")]
    public TimeSpan? EngineOperatingHours { get; set; }

    // transmission_temperature : REAL
    [Column("transmission_temperature")]
    public double? TransmissionTemperature { get; set; }

    // remaining_fuel : REAL
    [Column("remaining_fuel")]
    public double? RemainingFuel { get; set; }

    // remaining_fuel_real_time : REAL
    [Column("remaining_fuel_real_time")]
    public double? RemainingFuelRealTime { get; set; }

    // pressure_hydraulic_system : REAL
    [Column("pressure_hydraulic_system")]
    public double? PressureHydraulicSystem { get; set; }

    // hydraulic_fuid_temperature : REAL
    [Column("hydraulic_fluid_temperature")]
    public double? HydraulicFluidTemperature { get; set; }

    // battery_voltage : REAL
    [Column("battery_voltage")]
    public double? BatteryVoltage { get; set; }

    // latitude : NUMERIC(9,6)
    [Required]
    [Column("latitude")]
    public decimal Latitude { get; set; }

    // longitude : NUMERIC(9,6)
    [Required]
    [Column("longitude")]
    public decimal Longitude { get; set; }
}
}
