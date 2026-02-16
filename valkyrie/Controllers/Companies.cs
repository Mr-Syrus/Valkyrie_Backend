using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using valkyrie.Models;
using valkyrie.Models.Companies;

namespace valkyrie.Controllers;

public class Companies
{
    private readonly WebApplication _app;
    private readonly Auth _auth;

    public Companies(WebApplication app, RouteGroupBuilder router, Auth auth)
    {
        _app = app;
        _auth = auth;
        var routerCompanies = router.MapGroup("/companies");

        routerCompanies.MapGet("/all_name_companies", GetAllNameCompaniesApi);
        routerCompanies.MapPost("", CreteCompanyApi);
        routerCompanies.MapPut("", PutCompanyApi);
        routerCompanies.MapGet("/search", SearchByParentsApi);
    }

    private async Task<List<Company>> GetAllChildCompaniesRecursionByCompanyId(int parentId, AppDbContext db)
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
            var grandchildren = await GetAllChildCompaniesRecursionByCompanyId(childId, db);
            result.AddRange(grandchildren);
        }

        return result;
    }

    public async Task<List<Company>> GetAllChildCompaniesRecursionByUserId(int userId, AppDbContext db)
    {
        var result = new List<Company>();
        foreach (var companyId in await db.UserCompanies.Where(uc => uc.UserId == userId).Select(uc => uc.CompanyId)
                     .ToListAsync())
        {
            var grandchildren = await GetAllChildCompaniesRecursionByCompanyId(companyId, db);
            result.AddRange(grandchildren);
        }

        return result;
    }


    private async Task<IResult> GetAllNameCompaniesApi(HttpRequest request)
    {
        await using var scope = _app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var user = await _auth.GetUserBySession(request, db);
        if (user == null)
            return Results.Unauthorized();
        if (user.IsAdmin)
            return Results.Ok(await db.Companies.ToListAsync());

        return Results.Ok(GetAllChildCompaniesRecursionByUserId(user.Id, db));
    }

    private class CreteCompanyRequest
    {
        public string Name { get; set; } = default!;
        public string Parents { get; set; } = default!;
    }

    private async Task<IResult> CreteCompanyApi([FromBody] CreteCompanyRequest data, HttpRequest request)
    {
        await using var scope = _app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var user = await _auth.GetUserBySession(request, db);
        if (user == null)
            return Results.Unauthorized();

        Company? parentCompany = null;
        if (!string.IsNullOrEmpty(data.Parents))
        {
            parentCompany = await db.Companies.Where(c => c.Name == data.Parents).FirstOrDefaultAsync();
            if (parentCompany == null)
                return Results.BadRequest($"Компания с именем '{data.Parents}' не найдена.");
        }

        var companyDublicat = await db.Companies.Where(c => c.Name == data.Name).FirstOrDefaultAsync();
        if (companyDublicat != null)
        {
            return Results.BadRequest($"Компания с именем '{data.Name}' уже существует.");
        }

        async Task<int> crete()
        {
            var newCompuny = new Company
            {
                Name = data.Name
            };
            db.Companies.Add(newCompuny);
            if (parentCompany != null)
            {
                var newParentsCompany = new ParentsCompany
                {
                    CompanyParents = parentCompany,
                    CompanyChild = newCompuny
                };
                db.ParentsCompanies.Add(newParentsCompany);
            }

            await db.SaveChangesAsync();
            return newCompuny.Id;
        }

        if (user.IsAdmin || parentCompany == null)
        {
            return Results.Ok(new { id = crete() });
        }

        var companiesRuleOk = await GetAllChildCompaniesRecursionByUserId(user.Id, db);

        var ruleOk = companiesRuleOk.Any(c => c.Id == parentCompany.Id);
        if (!ruleOk)
        {
            return Results.Forbid();
        }

        return Results.Ok(new { id = await crete() });
    }

    private class PutCompanyRequest : CreteCompanyRequest
    {
        public int Id { get; set; } = default!;
    }

    private async Task<IResult> PutCompanyApi([FromBody] PutCompanyRequest data, HttpRequest request)
    {
        await using var scope = _app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var user = await _auth.GetUserBySession(request, db);
        if (user == null)
            return Results.Unauthorized();

        Company? parentCompany = null;
        if (!string.IsNullOrEmpty(data.Parents))
        {
            parentCompany = await db.Companies.Where(c => c.Name == data.Parents).FirstOrDefaultAsync();
            if (parentCompany == null)
                return Results.BadRequest($"Компания с именем '{data.Parents}' не найдена.");
        }
        
        var company = await db.Companies.Where(c => c.Id == data.Id ).FirstOrDefaultAsync();
        if (company == null)
        {
            return Results.BadRequest($"Компания не существует.");
        }

        if (company.Name != data.Name)
        {
            var companyDublicat = await db.Companies.Where(c => c.Name == data.Name ).FirstOrDefaultAsync();
            if (companyDublicat != null)
            {
                return Results.BadRequest($"Компания с именем '{data.Name}' уже существует.");
            }
        }

        async Task<int> put()
        {
            company.Name = data.Name;
            
            var oldLinks = db.ParentsCompanies.Where(pc => pc.Id == company.Id);
            db.ParentsCompanies.RemoveRange(oldLinks);
            
            if (parentCompany != null)
            {
                var newParentsCompany = new ParentsCompany
                {
                    CompanyParents = parentCompany,
                    CompanyChild = company
                };
                db.ParentsCompanies.Add(newParentsCompany);
            }

            await db.SaveChangesAsync();
            return company.Id;
        }

        if (user.IsAdmin || parentCompany == null)
        {
            return Results.Ok(new { id = put() });
        }

        var companiesRuleOk = await GetAllChildCompaniesRecursionByUserId(user.Id, db);

        var ruleOk = companiesRuleOk.Any(c => c.Id == parentCompany.Id);
        if (!ruleOk)
        {
            return Results.Forbid();
        }

        return Results.Ok(new { id = await put() });
    }


    private async Task<IResult> SearchByParentsApi([FromQuery] int[] ids, HttpRequest request)
    {
        await using var scope = _app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var user = await _auth.GetUserBySession(request, db);
        if (user == null)
            return Results.Unauthorized();

        var q = db.Companies
            .GroupJoin(
                db.ParentsCompanies,
                c => c.Id,
                pc => pc.Id,
                (c, pcs) => new { Company = c, ParentsCompanies = pcs.DefaultIfEmpty() }
            )
            .SelectMany(
                x => x.ParentsCompanies,
                (x, pc) => new { x.Company, ParentsCompany = pc }
            );

        if (!user.IsAdmin)
        {
            var companiesIds = (await GetAllChildCompaniesRecursionByUserId(user.Id, db))
                .Select(c => c.Id)
                .ToList();
            q = q.Where(c => companiesIds.Contains(c.Company.Id));
        }

        if (ids.Length != 0)
        {
            var idsList = ids.ToList();
            q = q.Where(cpc => cpc.ParentsCompany != null && idsList.Contains(cpc.ParentsCompany.CompanyId));
        }

        var result = await q
            .Select(cpc => new
            {
                CompanyId = cpc.Company.Id,
                CompanyName = cpc.Company.Name,
                ParentsCompanyName = cpc.ParentsCompany != null
                    ? cpc.ParentsCompany.CompanyParents.Name
                    : null
            })
            .ToListAsync();

        return Results.Ok(result);
    }
}