using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using valkyrie.Models;
using valkyrie.Models.Cars;

namespace valkyrie.Controllers;

public class Message
{
    private readonly WebApplication _app;
    private readonly Auth _auth;
    private readonly Companies _companies;

    public Message(WebApplication app, RouteGroupBuilder router, Auth auth, Companies companies)
    {
        _app = app;
        _auth = auth;
        _companies = companies;

        var routerMessage = router.MapGroup("/message");

        routerMessage.MapPost("", CreteMessageApi);
        routerMessage.MapPut("", PutMessageApi);
        routerMessage.MapGet("/search", SearchApi);
    }

    private class CreteMessageRequest
    {
        public DateTime StartDateOperation { get; set; } = default!;
        public DateTime? EndDateOperation { get; set; }
        public int ModelCarId { get; set; } = default!;
        public int PlatformId { get; set; } = default!;
        public string Number { get; set; } = default!;
    }

    private async Task<IResult> CreteMessageApi([FromBody] CreteMessageRequest data, HttpRequest request)
    {
        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        return Results.Ok();
    }

    private class PutMessageRequest : CreteMessageRequest
    {
        public int Id { get; set; } = default!;
    }

    private async Task<IResult> PutMessageApi([FromBody] PutMessageRequest data, HttpRequest request)
    {
        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        return Results.Ok();
    }

    public class RangeValue
    {
        public double Min { get; set; }
        public double Max { get; set; }
    }

    public class FilterModel
    {
        // Двигатель
        [JsonPropertyName("engine_torque")] public RangeValue? EngineTorque { get; set; }

        [JsonPropertyName("engine_load")] public RangeValue? EngineLoad { get; set; }

        [JsonPropertyName("engine_oil_pressure")]
        public RangeValue? EngineOilPressure { get; set; }

        [JsonPropertyName("engine_il_temperature")]
        public RangeValue? EngineIlTemperature { get; set; }

        [JsonPropertyName("exhaust_gas_temperature")]
        public RangeValue? ExhaustGasTemperature { get; set; }

        [JsonPropertyName("engine_operating_hours")]
        public RangeValue? EngineOperatingHours { get; set; }

        // Топливо
        [JsonPropertyName("remaining_fuel_real_time")]
        public RangeValue? RemainingFuelRealTime { get; set; }

        [JsonPropertyName("remaining_fuel")] public RangeValue? RemainingFuel { get; set; }

        // Гидравлика
        [JsonPropertyName("pressure_hydraulic_system")]
        public RangeValue? PressureHydraulicSystem { get; set; }

        [JsonPropertyName("hydraulic_fluid_temperature")]
        public RangeValue? HydraulicFluidTemperature { get; set; }

        // Эксплуатация и телематика
        [JsonPropertyName("geolocation")] public bool? Geolocation { get; set; }

        // Состояние и обслуживание
        [JsonPropertyName("battery_voltage")] public RangeValue? BatteryVoltage { get; set; }
    }

    private async Task<IResult> SearchApi(
        [FromQuery] string filterJson,
        HttpRequest request
    )
    {
        var filter = JsonSerializer.Deserialize<FilterModel>(filterJson);

        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        var query = db.Cars
            .Include(c => c.ModelCar)
                .ThenInclude(mc => mc.CarBrand)
            .Include(c => c.ModelCar)
                .ThenInclude(mc => mc.CarType)
            .Include(c => c.Platform)
            .Select(car => new
            {
                Car = car,
                Event = db.Events
                    .Where(e => e.CarId == car.Id)
                    .OrderByDescending(e => e.DateTime)
                    .FirstOrDefault()
            });


        // Применяем фильтры
        if (filter != null)
        {
            if (filter.EngineTorque != null)
            {
                query = query.Where(x =>
                    x.Event != null &&
                    x.Event.EngineTorque >= filter.EngineTorque.Min &&
                    x.Event.EngineTorque <= filter.EngineTorque.Max
                );
            }

            if (filter.EngineLoad != null)
            {
                query = query.Where(x =>
                    x.Event != null &&
                    x.Event.EngineLoad >= filter.EngineLoad.Min &&
                    x.Event.EngineLoad <= filter.EngineLoad.Max
                );
            }

            if (filter.EngineOilPressure != null)
            {
                query = query.Where(x =>
                    x.Event != null &&
                    x.Event.EngineOilPressure >= filter.EngineOilPressure.Min &&
                    x.Event.EngineOilPressure <= filter.EngineOilPressure.Max
                );
            }

            if (filter.EngineIlTemperature != null)
            {
                query = query.Where(x =>
                    x.Event != null &&
                    x.Event.EngineILTemperature >= filter.EngineIlTemperature.Min &&
                    x.Event.EngineILTemperature <= filter.EngineIlTemperature.Max
                );
            }

            if (filter.ExhaustGasTemperature != null)
            {
                query = query.Where(x =>
                    x.Event != null &&
                    x.Event.ExhaustGasTemperature >= filter.ExhaustGasTemperature.Min &&
                    x.Event.ExhaustGasTemperature <= filter.ExhaustGasTemperature.Max
                );
            }

            if (filter.EngineOperatingHours != null)
            {
                query = query.Where(x =>
                    x.Event != null &&
                    x.Event.EngineOperatingHours.HasValue &&
                    x.Event.EngineOperatingHours.Value.TotalHours >= filter.EngineOperatingHours.Min &&
                    x.Event.EngineOperatingHours.Value.TotalHours <= filter.EngineOperatingHours.Max
                );
            }

            if (filter.RemainingFuelRealTime != null)
            {
                query = query.Where(x =>
                    x.Event != null &&
                    x.Event.RemainingFuelRealTime >= filter.RemainingFuelRealTime.Min &&
                    x.Event.RemainingFuelRealTime <= filter.RemainingFuelRealTime.Max
                );
            }

            if (filter.RemainingFuel != null)
            {
                query = query.Where(x =>
                    x.Event != null &&
                    x.Event.RemainingFuel >= filter.RemainingFuel.Min &&
                    x.Event.RemainingFuel <= filter.RemainingFuel.Max
                );
            }

            if (filter.PressureHydraulicSystem != null)
            {
                query = query.Where(x =>
                    x.Event != null &&
                    x.Event.PressureHydraulicSystem >= filter.PressureHydraulicSystem.Min &&
                    x.Event.PressureHydraulicSystem <= filter.PressureHydraulicSystem.Max
                );
            }

            if (filter.HydraulicFluidTemperature != null)
            {
                query = query.Where(x =>
                    x.Event != null &&
                    x.Event.HydraulicFluidTemperature >= filter.HydraulicFluidTemperature.Min &&
                    x.Event.HydraulicFluidTemperature <= filter.HydraulicFluidTemperature.Max
                );
            }

            if (filter.Geolocation.HasValue && filter.Geolocation.Value)
            {
                query = query.Where(x => x.Event != null); // оставляем только машины с координатами
            }

            if (filter.BatteryVoltage != null)
            {
                query = query.Where(x =>
                    x.Event != null &&
                    x.Event.BatteryVoltage >= filter.BatteryVoltage.Min &&
                    x.Event.BatteryVoltage <= filter.BatteryVoltage.Max
                );
            }
        }

        var result = await query.ToListAsync();

        return Results.Ok(result);
    }
}