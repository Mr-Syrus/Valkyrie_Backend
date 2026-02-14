using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using valkyrie.Models;
using valkyrie.Models.Companies;
using valkyrie.Models.Users;

namespace valkyrie.Сontrollers;

public class Users
{
    private readonly WebApplication _app;
    private readonly Auth _auth;
    private readonly Companies _companies;

    public Users(WebApplication app, RouteGroupBuilder router, Auth auth, Companies companies)
    {
        _app = app;
        _auth = auth;
        _companies = companies;

        var routerUsers = router.MapGroup("/users");
        routerUsers.MapGet("/posts", GetPostApi);

        routerUsers.MapPost("", CreteUserApi);
        routerUsers.MapPut("", PutUserApi);
        routerUsers.MapGet("/search", SearchApi);
    }

    private async Task<List<User>> GetAllChildUsersByUserId(int userId, AppDbContext db)
    {
        var companies = await _companies.GetAllChildCompaniesRecursionByUserId(userId, db);

        var companiesIDS = companies.Select(c => c.Id).Distinct().ToList();

        var users = await db.Users.Join(
                db.UserCompanies,
                u => u.Id,
                uc => uc.UserId,
                (u, uc) => new { U = u, UC = uc }
            ).Where(x => companiesIDS.Contains(x.UC.CompanyId))
            .Select(x => x.U).ToListAsync();
        return users;
    }

    private async Task<IResult> GetPostApi(HttpRequest request)
    {
        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        return Results.Ok(await db.PostTypes.ToListAsync());
    }

    private class CreteUserRequest
    {
        public string Username { get; set; } = default!;
        public string Firstname { get; set; } = default!;
        public string Lastname { get; set; } = default!;
        public string Surname { get; set; } = default!;
        public string Password { get; set; } = default!;
        public bool Decommissioned { get; set; } = default!;
        public string Company { get; set; } = default!;
        public string Post { get; set; } = default!;
    }

    public async Task<PostType> GetOrCreatePostTypeAsync(string name, AppDbContext _context)
    {
        // нормализация (по желанию)
        name = name.Trim();

        var postType = await _context.Set<PostType>()
            .FirstOrDefaultAsync(p => p.Name == name);

        if (postType != null)
            return postType;

        postType = new PostType
        {
            Name = name
        };

        _context.Add(postType);
        await _context.SaveChangesAsync();

        return postType;
    }

    private async Task<IResult> CreteUserApi([FromBody] CreteUserRequest data, HttpRequest request)
    {
        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        var userSession = await _auth.GetUserBySession(request, db);
        if (userSession == null)
            return Results.Unauthorized();

        var userDublicat = await db.Users.Where(c => c.Username == data.Username).FirstOrDefaultAsync();
        if (userDublicat != null)
        {
            return Results.BadRequest($"User с именем '{data.Username}' уже существует.");
        }

        Company? company = null;
        if (!string.IsNullOrEmpty(data.Company))
        {
            company = await db.Companies.Where(c => c.Name == data.Company).FirstOrDefaultAsync();
            if (company == null)
                return Results.BadRequest($"Компания с именем '{data.Company}' не найдена.");
        }

        var postType = await GetOrCreatePostTypeAsync(data.Post, db);

        async Task<int> crete()
        {
            var newUser = new User
            {
                Username = data.Username,
                Firstname = data.Firstname,
                Lastname = data.Lastname,
                Surname = data.Surname,
                Decommissioned = data.Decommissioned,

                HashPassword = Auth.Sha256(data.Password),
                PostTypeId = postType.Id,
            };
            db.Users.Add(newUser);

            if (company != null)
                db.UserCompanies.Add(new UserCompany
                {
                    Company = company,
                    User = newUser,
                });

            await db.SaveChangesAsync();
            return newUser.Id;
        }

        // if (userSession.IsAdmin)
        // {
        //     return Results.Ok(new { id = crete() });
        // }

        return Results.Ok(new { id = await crete() });
    }

    private class PutUserRequest : CreteUserRequest
    {
        public int Id { get; set; } = default!;
    }

    private async Task<IResult> PutUserApi([FromBody] PutUserRequest data, HttpRequest request)
    {
        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        var userSession = await _auth.GetUserBySession(request, db);
        if (userSession == null)
            return Results.Unauthorized();

        var user = await db.Users.Where(u => u.Id == data.Id).FirstOrDefaultAsync();
        if (user == null)
        {
            return Results.BadRequest($"User с id '{data.Id}' нет.");
        }

        if (user.Username != data.Username)
        {
            var userDublicat = await db.Users.Where(c => c.Username == data.Username).FirstOrDefaultAsync();
            if (userDublicat != null)
            {
                return Results.BadRequest($"User с именем '{data.Username}' уже существует.");
            }
        }

        Company? company = null;
        if (!string.IsNullOrEmpty(data.Company))
        {
            company = await db.Companies.Where(c => c.Name == data.Company).FirstOrDefaultAsync();
            if (company == null)
                return Results.BadRequest($"Компания с именем '{data.Company}' не найдена.");
        }

        var postType = await GetOrCreatePostTypeAsync(data.Post, db);

        async Task<int> put()
        {
            user.Username = data.Username;
            user.Firstname = data.Firstname;
            user.Lastname = data.Lastname;
            user.Surname = data.Surname;
            user.Decommissioned = data.Decommissioned;

            user.HashPassword = Auth.Sha256(data.Password);
            user.PostTypeId = postType.Id;
            
            var oldLinks = db.UserCompanies.Where(pc => pc.UserId == user.Id);
            db.UserCompanies.RemoveRange(oldLinks);
            
            if (company != null)
                db.UserCompanies.Add(new UserCompany
                {
                    Company = company,
                    User = user,
                });

            await db.SaveChangesAsync();
            return user.Id;
        }


        return Results.Ok(new { id = await put() });
    }

    private async Task<IResult> SearchApi(
        [FromQuery] int[]? idsCompany,
        [FromQuery] int[]? idsPost,
        HttpRequest request
    )
    {
        idsCompany = idsCompany ?? [];
        idsPost = idsPost ?? [];

        var db = _app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await _auth.GetUserBySession(request, db);
        if (user == null)
            return Results.Unauthorized();

        var q = db.Users
            .GroupJoin(
                db.UserCompanies,
                u => u.Id,
                uc => uc.UserId,
                (u, ucs) => new { U = u, UCs = ucs }
            )
            .SelectMany(
                x => x.UCs.DefaultIfEmpty(),
                (x, uc) => new { x.U, UC = uc }
            )
            .GroupJoin(
                db.Companies,
                x => x.UC != null ? x.UC.CompanyId : 0, // если UC null, не будет совпадений
                c => c.Id,
                (x, cs) => new { x.U, x.UC, Cs = cs }
            )
            .SelectMany(
                x => x.Cs.DefaultIfEmpty(),
                (x, c) => new { U = x.U, C = c } // U всегда есть, C может быть null
            ).Join(
                db.PostTypes,
                uuc => uuc.U.PostTypeId,
                pt => pt.Id,
                (uuc, pt) => new { U = uuc.U, C = uuc.C, PT = pt }
            );


        if (!user.IsAdmin)
        {
            var companiesIds = (await _companies.GetAllChildCompaniesRecursionByUserId(user.Id, db))
                .Select(c => c.Id)
                .ToList();

            var usersIds = await db.UserCompanies.Where(uc => companiesIds.Contains(uc.CompanyId))
                .Select(uc => uc.UserId)
                .ToListAsync();

            q = q.Where(u => usersIds.Contains(u.U.Id));
        }

        if (idsPost.Length != 0)
        {
            var idsList = idsPost.ToList();
            q = q.Where(ucpt => idsList.Contains(ucpt.U.PostTypeId));
        }

        if (idsCompany.Length != 0)
        {
            var idsList = idsCompany.ToList();
            q = q.Where(ucpt => ucpt.C != null && idsList.Contains(ucpt.C.Id));
        }

        var result = await q.Select(ucpt => new
            {
                User = ucpt.U,
                Company = ucpt.C,
                PostType = ucpt.PT
            }
        ).ToListAsync();

        return Results.Ok(result);
    }
}