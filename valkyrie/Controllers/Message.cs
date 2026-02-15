using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using valkyrie.Models;
using valkyrie.Models.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace valkyrie.Controllers;

public class Message
{
    private readonly WebApplication _app;
    private readonly Auth _auth;

    public Message(WebApplication app, RouteGroupBuilder router, Auth auth)
    {
        _app = app;
        _auth = auth;

        var routerMessage = router.MapGroup("/message");
        routerMessage.MapGet("/search", SearchApi);
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

        var user = await _auth.GetUserBySession(request, db);
        if (user == null)
            return Results.Unauthorized();

        var query = db.Events
            .Include(e => e.TypeEvent)
            .Include(e => e.Car)
                .ThenInclude(c => c.ModelCar)
                    .ThenInclude(mc => mc.CarBrand)
            .Include(e => e.Car)
                .ThenInclude(c => c.ModelCar)
                    .ThenInclude(mc => mc.CarType)
            .Include(e => e.Platforms)
            .GroupJoin(
                db.Histories,
                e => e.Id,
                h => h.EventId,
                (e, histories) => new { Event = e, Histories = histories }
            )
            .SelectMany(
                x => x.Histories.DefaultIfEmpty(),
                (x, history) => new
                {
                    Event = x.Event,
                    History = history
                }
            );

        // Применяем фильтры
        if (filter != null)
        {
            if (filter.EngineTorque != null)
            {
                query = query.Where(x =>
                    x.Event.EngineTorque >= filter.EngineTorque.Min &&
                    x.Event.EngineTorque <= filter.EngineTorque.Max
                );
            }

            if (filter.EngineLoad != null)
            {
                query = query.Where(x =>
                    x.Event.EngineLoad >= filter.EngineLoad.Min &&
                    x.Event.EngineLoad <= filter.EngineLoad.Max
                );
            }

            if (filter.EngineOilPressure != null)
            {
                query = query.Where(x =>
                    x.Event.EngineOilPressure >= filter.EngineOilPressure.Min &&
                    x.Event.EngineOilPressure <= filter.EngineOilPressure.Max
                );
            }

            if (filter.EngineIlTemperature != null)
            {
                query = query.Where(x =>
                    x.Event.EngineILTemperature >= filter.EngineIlTemperature.Min &&
                    x.Event.EngineILTemperature <= filter.EngineIlTemperature.Max
                );
            }

            if (filter.ExhaustGasTemperature != null)
            {
                query = query.Where(x =>
                    x.Event.ExhaustGasTemperature >= filter.ExhaustGasTemperature.Min &&
                    x.Event.ExhaustGasTemperature <= filter.ExhaustGasTemperature.Max
                );
            }

            if (filter.EngineOperatingHours != null)
            {
                query = query.Where(x =>
                    x.Event.EngineOperatingHours.HasValue &&
                    x.Event.EngineOperatingHours.Value.TotalHours >= filter.EngineOperatingHours.Min &&
                    x.Event.EngineOperatingHours.Value.TotalHours <= filter.EngineOperatingHours.Max
                );
            }

            if (filter.RemainingFuelRealTime != null)
            {
                query = query.Where(x =>
                    x.Event.RemainingFuelRealTime >= filter.RemainingFuelRealTime.Min &&
                    x.Event.RemainingFuelRealTime <= filter.RemainingFuelRealTime.Max
                );
            }

            if (filter.RemainingFuel != null)
            {
                query = query.Where(x =>
                    x.Event.RemainingFuel >= filter.RemainingFuel.Min &&
                    x.Event.RemainingFuel <= filter.RemainingFuel.Max
                );
            }

            if (filter.PressureHydraulicSystem != null)
            {
                query = query.Where(x =>
                    x.Event.PressureHydraulicSystem >= filter.PressureHydraulicSystem.Min &&
                    x.Event.PressureHydraulicSystem <= filter.PressureHydraulicSystem.Max
                );
            }

            if (filter.HydraulicFluidTemperature != null)
            {
                query = query.Where(x =>
                    x.Event.HydraulicFluidTemperature >= filter.HydraulicFluidTemperature.Min &&
                    x.Event.HydraulicFluidTemperature <= filter.HydraulicFluidTemperature.Max
                );
            }

            if (filter.Geolocation.HasValue && filter.Geolocation.Value)
            {
                // оставляем только события с координатами (всегда есть)
                query = query.Where(x => x.Event != null);
            }

            if (filter.BatteryVoltage != null)
            {
                query = query.Where(x =>
                    x.Event.BatteryVoltage >= filter.BatteryVoltage.Min &&
                    x.Event.BatteryVoltage <= filter.BatteryVoltage.Max
                );
            }
        }

        var result = await query
            .OrderByDescending(x => x.Event.DateTime)
            .Take(100) // Ограничиваем 100 последними записями
            .ToListAsync();

        return Results.Ok(result);
    }
}
