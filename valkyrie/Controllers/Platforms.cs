using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using valkyrie.Models;
using valkyrie.Models.Companies;
using valkyrie.Models.Users;

namespace valkyrie.Controllers;

public class Platforms
{
    private readonly WebApplication _app;
    private readonly Auth _auth;
    private readonly Companies _companies;

    public Platforms(WebApplication app, RouteGroupBuilder router, Auth auth, Companies companies)
    {
        _app = app;
        _auth = auth;
        _companies = companies;

        var routerPlatforms = router.MapGroup("/platforms");

        routerPlatforms.MapPost("", CretePlatformsApi);
        routerPlatforms.MapPut("", PutPlatformsApi);
        routerPlatforms.MapGet("/search", SearchApi);
    }

    private class CretePlatformsRequest
    {
        public string Name { get; set; } = default!;
        public string Address { get; set; } = default!;
        public DateTimeOffset StartDate { get; set; } = default!;
        public DateTimeOffset? EndDate { get; set; } = default!;
        public string Company { get; set; } = default!;
    }

    private async Task<IResult> CretePlatformsApi([FromBody] CretePlatformsRequest data, HttpRequest request)
    {
        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        var userSession = await _auth.GetUserBySession(request, db);
        if (userSession == null)
            return Results.Unauthorized();

        var platformDublicat = await db.Platforms.Where(c => c.Name == data.Name).FirstOrDefaultAsync();
        if (platformDublicat != null)
        {
            return Results.BadRequest($"platform с именем '{data.Name}' уже существует.");
        }

        Company company = await db.Companies.Where(c => c.Name == data.Company).FirstOrDefaultAsync();
        if (company == null)
            return Results.BadRequest($"Компания с именем '{data.Company}' не найдена.");


        async Task<int> crete()
        {
            var newPlatform = new Platform
            {
                Name = data.Name,
                Address = data.Address,
                StartDate = data.StartDate.ToUniversalTime(),
                EndDate = data.EndDate?.ToUniversalTime(),
                CompanyId = company.Id,
            };
            db.Platforms.Add(newPlatform);

            await db.SaveChangesAsync();
            return newPlatform.Id;
        }

        return Results.Ok(new { id = await crete() });
    }

    private class PutPlatformsRequest : CretePlatformsRequest
    {
        public int Id { get; set; } = default!;
    }

    private async Task<IResult> PutPlatformsApi([FromBody] PutPlatformsRequest data, HttpRequest request)
    {
        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        var userSession = await _auth.GetUserBySession(request, db);
        if (userSession == null)
            return Results.Unauthorized();

        var platform = await db.Platforms.Where(u => u.Id == data.Id).FirstOrDefaultAsync();
        if (platform == null)
        {
            return Results.BadRequest($"platform с id '{data.Id}' нет.");
        }

        if (platform.Name != data.Name)
        {
            var userDublicat = await db.Platforms.Where(c => c.Name == data.Name).FirstOrDefaultAsync();
            if (userDublicat != null)
            {
                return Results.BadRequest($"User с именем '{data.Name}' уже существует.");
            }
        }

        Company company = await db.Companies.Where(c => c.Name == data.Company).FirstOrDefaultAsync();
        if (company == null)
            return Results.BadRequest($"Компания с именем '{data.Company}' не найдена.");

        async Task<int> put()
        {
            platform.Name = data.Name;
            platform.Address = data.Address;
            platform.StartDate = data.StartDate.ToUniversalTime();
            platform.EndDate = data.EndDate?.ToUniversalTime();
            platform.CompanyId = company.Id;

            await db.SaveChangesAsync();
            return platform.Id;
        }

        return Results.Ok(new { id = await put() });
    }

    private async Task<IResult> SearchApi(
        [FromQuery] int[]? idsCompany,
        HttpRequest request
    )
    {
        idsCompany ??= [];

        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await _auth.GetUserBySession(request, db);
        if (user == null)
            return Results.Unauthorized();

        IQueryable<Platform> q = db.Platforms.Include(p => p.Company);


        if (!user.IsAdmin)
        {
            var companiesIds = (await _companies.GetAllChildCompaniesRecursionByUserId(user.Id, db))
                .Select(c => c.Id)
                .ToList();

            q = q.Where(p => companiesIds.Contains(p.CompanyId));
        }
        
        if (idsCompany.Length != 0)
        {
            var idsList = idsCompany.ToList();
            q = q.Where(p => idsList.Contains(p.CompanyId));
        }

        var result = await q.ToListAsync();

        return Results.Ok(result);
    }
}