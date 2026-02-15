using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using valkyrie.Models;
using valkyrie.Models.Cars;
using valkyrie.Models.Companies;
using valkyrie.Models.Users;

namespace valkyrie.Controllers;

public class Cars
{
    private readonly WebApplication _app;
    private readonly Auth _auth;
    private readonly Companies _companies;

    public Cars(WebApplication app, RouteGroupBuilder router, Auth auth, Companies companies)
    {
        _app = app;
        _auth = auth;
        _companies = companies;

        var routerCars = router.MapGroup("/cars");

        routerCars.MapGet("model_cars", GetModelCarsApi);
        routerCars.MapPost("", CreteCarsApi);
        routerCars.MapPut("", PutCarsApi);
        routerCars.MapGet("/search", SearchApi);
    }

    private async Task<IResult> GetModelCarsApi(HttpRequest request)
    {
        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        var res = await db.ModelCars
            .Include(mc => mc.CarBrand)
            .Include(mc => mc.CarType)
            .ToListAsync();

        return Results.Ok(res);
    }

    private class CreteCarsRequest
    {
        public DateTime StartDateOperation { get; set; } = default!;
        public DateTime? EndDateOperation { get; set; }
        public int ModelCarId { get; set; } = default!;
        public int PlatformId { get; set; } = default!;
        public string Number { get; set; } = default!;
    }

    private async Task<IResult> CreteCarsApi([FromBody] CreteCarsRequest data, HttpRequest request)
    {
        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        var userSession = await _auth.GetUserBySession(request, db);
        if (userSession == null)
            return Results.Unauthorized();

        // Проверка существования ModelCar
        var modelCar = await db.ModelCars.Where(mc => mc.Id == data.ModelCarId).FirstOrDefaultAsync();
        if (modelCar == null)
        {
            return Results.BadRequest($"Модель машины с id '{data.ModelCarId}' не найдена.");
        }

        // Проверка существования Platform
        var platform = await db.Platforms.Where(p => p.Id == data.PlatformId).FirstOrDefaultAsync();
        if (platform == null)
        {
            return Results.BadRequest($"Платформа с id '{data.PlatformId}' не найдена.");
        }

        // Проверка уникальности номера машины
        var carDuplicate = await db.Cars.Where(c => c.Number == data.Number).FirstOrDefaultAsync();
        if (carDuplicate != null)
        {
            return Results.BadRequest($"Машина с номером '{data.Number}' уже существует.");
        }

        async Task<int> create()
        {
            var newCar = new Car
            {
                StartDateOperation = DateTime.SpecifyKind(data.StartDateOperation, DateTimeKind.Utc),
                EndDateOperation = data.EndDateOperation.HasValue 
                    ? DateTime.SpecifyKind(data.EndDateOperation.Value, DateTimeKind.Utc) 
                    : null,
                ModelCarId = data.ModelCarId,
                PlatformId = data.PlatformId,
                Number = data.Number,
                Decommissioned = false
            };

            db.Cars.Add(newCar);
            await db.SaveChangesAsync();
            return newCar.Id;
        }

        return Results.Ok(new { id = await create() });
    }

    private class PutCarsRequest : CreteCarsRequest
    {
        public int Id { get; set; } = default!;
    }

    private async Task<IResult> PutCarsApi([FromBody] PutCarsRequest data, HttpRequest request)
    {
        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        var userSession = await _auth.GetUserBySession(request, db);
        if (userSession == null)
            return Results.Unauthorized();

        // Проверка существования машины
        var car = await db.Cars.Where(c => c.Id == data.Id).FirstOrDefaultAsync();
        if (car == null)
        {
            return Results.BadRequest($"Машина с id '{data.Id}' не найдена.");
        }

        // Проверка существования ModelCar
        var modelCar = await db.ModelCars.Where(mc => mc.Id == data.ModelCarId).FirstOrDefaultAsync();
        if (modelCar == null)
        {
            return Results.BadRequest($"Модель машины с id '{data.ModelCarId}' не найдена.");
        }

        // Проверка существования Platform
        var platform = await db.Platforms.Where(p => p.Id == data.PlatformId).FirstOrDefaultAsync();
        if (platform == null)
        {
            return Results.BadRequest($"Платформа с id '{data.PlatformId}' не найдена.");
        }

        // Проверка уникальности номера машины (если номер изменился)
        if (car.Number != data.Number)
        {
            var carDuplicate = await db.Cars.Where(c => c.Number == data.Number).FirstOrDefaultAsync();
            if (carDuplicate != null)
            {
                return Results.BadRequest($"Машина с номером '{data.Number}' уже существует.");
            }
        }

        async Task<int> update()
        {
            car.StartDateOperation = DateTime.SpecifyKind(data.StartDateOperation, DateTimeKind.Utc);
            car.EndDateOperation = data.EndDateOperation.HasValue 
                ? DateTime.SpecifyKind(data.EndDateOperation.Value, DateTimeKind.Utc) 
                : null;
            car.ModelCarId = data.ModelCarId;
            car.PlatformId = data.PlatformId;
            car.Number = data.Number;

            await db.SaveChangesAsync();
            return car.Id;
        }

        return Results.Ok(new { id = await update() });
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