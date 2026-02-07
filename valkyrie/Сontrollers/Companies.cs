using Microsoft.EntityFrameworkCore;
using valkyrie.Models;
using valkyrie.Models.Companies;

namespace valkyrie.Сontrollers;

public class Companies
{
    private readonly WebApplication _app;
    private readonly Auth _auth;

    public Companies(WebApplication app, RouteGroupBuilder router, Auth auth)
    {
        _app = app;
        _auth = auth;
        var routerCompanies = router.MapGroup("/companies");

        routerCompanies.MapGet("/all_name_companies", GetAllNameCompanies);

    }

    private async Task<List<Company>> GetAllChildCompaniesRecursion(int parentId, AppDbContext db)
    {
        var result = new List<Company>();

        // Получаем прямых детей
        var directChildrenIds = await db.ParentsCompanies
            .Where(pc => pc.CompanyId == parentId)
            .Select(pc => pc.Id)
            .ToListAsync();

        if (!directChildrenIds.Any())
            return result;

        // Получаем объекты Company для этих детей
        var directChildrenCompanies = await db.Companies
            .Where(c => directChildrenIds.Contains(c.Id))
            .ToListAsync();

        result.AddRange(directChildrenCompanies);

        // Рекурсивно ищем детей детей
        foreach (var childId in directChildrenIds)
        {
            var grandchildren = await GetAllChildCompaniesRecursion(childId, db);
            result.AddRange(grandchildren);
        }

        return result;
    }
    
    private async Task<IResult> GetAllNameCompanies(HttpRequest request)
    {
        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await _auth.GetUserBySession(request, db);
        if (user == null)
            return Results.Unauthorized();
        if (user.IsAdmin)
            return Results.Ok(await db.Companies.ToListAsync());
        
        var result = new List<Company>();
        foreach (var companyId in await db.UserCompanies.Where(uc=>uc.UserId == user.Id).Select(uc=>uc.CompanyId).ToListAsync())
        {
            var grandchildren = await GetAllChildCompaniesRecursion(companyId, db);
            result.AddRange(grandchildren);
        }
        return Results.Ok(result);
    }
    
}