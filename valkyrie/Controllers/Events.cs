using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using valkyrie.Models;
using valkyrie.Models.Events;
using valkyrie.Models.Users;
using valkyrie.Models.Companies;

namespace valkyrie.Controllers;

public class Events
{
    private readonly WebApplication _app;
    private static readonly Random Rng = new();

    public Events(WebApplication app, RouteGroupBuilder router, Auth auth, Companies companies)
    {
        _app = app;

        var routerEvents = router.MapGroup("/events");

        routerEvents.MapPost("", CreteApi);
        routerEvents.MapPost("/ok", OkApi);

        var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        if (!db.TypeEvents.Any(te => te.Name == "SOS"))
        {
            db.TypeEvents.Add(new TypeEvent
            {
                Name = "SOS"
            });
            db.SaveChanges();
        }

        _ = Task.Run(SimulatorLoopAsync);
    }

    private static double RandDouble(double min, double max) =>
        Math.Round(min + Rng.NextDouble() * (max - min), 1);

    private static CreteEventsRequest BuildRequest(string carNumber, string eventType) => new()
    {
        CarNumber = carNumber,
        TypeEventName = eventType,
        EngineTorque = RandDouble(0, 2500),
        EngineLoad = RandDouble(0, 100),
        EngineOilPressure = RandDouble(0, 10),
        EngineILTemperature = RandDouble(-20, 140),
        ExhaustGasTemperature = RandDouble(0, 1000),
        EngineOperatingHours = RandDouble(0, 100000),
        RemainingFuel = RandDouble(0, 100),
        RemainingFuelRealTime = RandDouble(0, 100),
        PressureHydraulicSystem = RandDouble(0, 400),
        HydraulicFluidTemperature = RandDouble(-20, 120),
        BatteryVoltage = RandDouble(9, 36),
        Latitude = 53.7710877m,
        Longitude = 87.1262593m
    };

    private async Task SimulatorLoopAsync()
    {
        var tickCount = 0;

        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(30));

            try
            {
                using var scope = _app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var cars = await db.Cars
                    .Where(c => !c.Decommissioned)
                    .ToListAsync();

                if (cars.Count == 0) continue;

                tickCount++;
                var car = cars[Rng.Next(cars.Count)];
                var eventType = tickCount % 4 == 0 ? "SOS" : "INFO";

                await CreateEventAsync(BuildRequest(car.Number, eventType));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Simulator error: {ex.Message}");
            }
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
        public double? EngineOperatingHours { get; set; }
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
        return await CreateEventAsync(data);
    }

    private async Task<IResult> CreateEventAsync(CreteEventsRequest data)
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
            EngineOperatingHours = data.EngineOperatingHours.HasValue
                ? TimeSpan.FromHours(data.EngineOperatingHours.Value)
                : null,
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
            var companyHierarchy = await GetCompanyHierarchy(db, car.PlatformId);
            var userIds = await GetUsersFromCompanies(db, companyHierarchy);

            _ = Task.Run(async () => await ProcessSOSNotifications(newEvent.Id, userIds));
        }

        return Results.Ok(new
        {
            id = newEvent.Id
        });
    }

    private async Task<List<int>> GetCompanyHierarchy(AppDbContext db, int platformId)
    {
        var platform = await db.Platforms.FirstOrDefaultAsync(p => p.Id == platformId);
        if (platform == null) return new List<int>();

        var companyIds = new List<int> { platform.CompanyId };
        var currentCompanyId = platform.CompanyId;

        while (true)
        {
            var parentCompany = await db.ParentsCompanies
                .FirstOrDefaultAsync(pc => pc.Id == currentCompanyId);

            if (parentCompany == null) break;

            companyIds.Add(parentCompany.CompanyId);
            currentCompanyId = parentCompany.CompanyId;
        }

        return companyIds;
    }

    private async Task<List<int>> GetUsersFromCompanies(AppDbContext db, List<int> companyIds)
    {
        var userIds = new List<int>();

        foreach (var companyId in companyIds)
        {
            var users = await db.UserCompanies
                .Where(uc => uc.CompanyId == companyId)
                .Select(uc => uc.UserId)
                .ToListAsync();

            userIds.AddRange(users);
        }

        var adminIds = await db.Users
            .Where(u => u.IsAdmin && !u.Decommissioned && !userIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync();

        userIds.AddRange(adminIds);

        return userIds;
    }

    private async Task ProcessSOSNotifications(int eventId, List<int> userIds)
    {
        using var scope = _app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var eventData = await db.Events
            .Include(e => e.Car)
            .ThenInclude(c => c.ModelCar)
            .ThenInclude(mc => mc.CarBrand)
            .Include(e => e.TypeEvent)
            .Include(e => e.Platforms)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventData == null)
        {
            Console.WriteLine("ERROR");
            return;
        }

        foreach (var userId in userIds)
        {
            var existingHistory = await db.Histories
                .FirstOrDefaultAsync(h => h.EventId == eventId && h.Answer == true);

            if (existingHistory != null)
            {
                break;
            }

            var history = new History
            {
                EventId = eventId,
                Answer = false,
                DataTime = DateTimeOffset.UtcNow,
                UserId = userId
            };
            db.Histories.Add(history);
            await db.SaveChangesAsync();


            var isOk = await Message.SendEventToUser(userId, eventData, history);

            if (isOk)
                await Task.Delay(TimeSpan.FromMinutes(10));
        }
    }

    class OkRequest
    {
        public int Id { get; set; }
    }

    private async Task<IResult> OkApi([FromBody] OkRequest data, HttpRequest request)
    {
        using var scope = _app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var history = await db.Histories.FirstOrDefaultAsync(h => h.Id == data.Id);

        if (history == null)
        {
             return Results.BadRequest("History not found");
        }
        
        history.Answer = true;
        
        await db.SaveChangesAsync();
        
        return Results.Ok(new
        {
            id = history.Id
        });
    }
}