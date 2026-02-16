using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using valkyrie.Models;
using valkyrie.Models.Events;
using valkyrie.Models.Users;

namespace valkyrie.Controllers;

public class Events
{
    private readonly WebApplication _app;

    public Events(WebApplication app, RouteGroupBuilder router, Auth auth, Companies companies)
    {
        _app = app;
        
        var routerEvents = router.MapGroup("/events");
        
        routerEvents.MapPost("", CreteApi);
        
        var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        if (!db.TypeEvents.Any(te => te.Name=="SOS"))
        {
            db.TypeEvents.Add(new TypeEvent
            {
                Name = "SOS"
            });
            db.SaveChanges();
        }
    }

    private class CreteEventsRequest
    {
        public string CarNumber { get; set; } = default!;
        public string TypeEventName { get; set; } = default!;
        public double? EngineTorque { get; set; }
        public double? EngineLoad { get; set; }
        public double? EngineOilPressure { get; set; }
        public double? EngineILTemperature { get; set; }
        public double? ExhaustGasTemperature { get; set; }
        public TimeSpan? EngineOperatingHours { get; set; }
        public double? TransmissionTemperature { get; set; }
        public double? RemainingFuel { get; set; }
        public double? RemainingFuelRealTime { get; set; }
        public double? PressureHydraulicSystem { get; set; }
        public double? HydraulicFluidTemperature { get; set; }
        public double? BatteryVoltage { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    private async Task<IResult> CreteApi([FromBody] CreteEventsRequest data, HttpRequest request)
    {
        using var scope = _app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var car = await db.Cars.FirstOrDefaultAsync(c => c.Number == data.CarNumber);
        if (car == null)
        {
            return Results.BadRequest(new { error = "Car not found" });
        }

        var typeEvent = await db.TypeEvents.FirstOrDefaultAsync(te => te.Name == data.TypeEventName);
        if (typeEvent == null)
        {
            typeEvent = new TypeEvent
            {
                Name = data.TypeEventName
            };
            db.TypeEvents.Add(typeEvent);
            await db.SaveChangesAsync();
        }

        var newEvent = new Event
        {
            PlatformId = car.PlatformId,
            CarId = car.Id,
            DateTime = DateTimeOffset.UtcNow,
            TypeEventId = typeEvent.Id,
            EngineTorque = data.EngineTorque,
            EngineLoad = data.EngineLoad,
            EngineOilPressure = data.EngineOilPressure,
            EngineILTemperature = data.EngineILTemperature,
            ExhaustGasTemperature = data.ExhaustGasTemperature,
            EngineOperatingHours = data.EngineOperatingHours,
            TransmissionTemperature = data.TransmissionTemperature,
            RemainingFuel = data.RemainingFuel,
            RemainingFuelRealTime = data.RemainingFuelRealTime,
            PressureHydraulicSystem = data.PressureHydraulicSystem,
            HydraulicFluidTemperature = data.HydraulicFluidTemperature,
            BatteryVoltage = data.BatteryVoltage,
            Latitude = data.Latitude,
            Longitude = data.Longitude
        };

        db.Events.Add(newEvent);
        await db.SaveChangesAsync();

        if (data.TypeEventName.Equals("SOS", StringComparison.OrdinalIgnoreCase))
        {
            var history = new History
            {
                EventId = newEvent.Id,
                Answer = false,
                DataTime = DateTimeOffset.UtcNow,
                UserId = 1
            };
            db.Histories.Add(history);
            await db.SaveChangesAsync();
        }

        return Results.Ok(new { 
            id = newEvent.Id 
        });
    }
}