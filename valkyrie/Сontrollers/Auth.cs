using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using valkyrie.Models;
using valkyrie.Models.Users;

namespace valkyrie.Сontrollers
{
    public class Auth
    {
        private readonly AppDbContext _db;

        public Auth(WebApplication app, RouteGroupBuilder router)
        {
            var routerAuth = router.MapGroup("/auth");

            routerAuth.MapPost("/login", LoginApi);
            routerAuth.MapGet("/check-session", CheckSessionApi);
            routerAuth.MapGet("/user", GetUserBySessionApi);

            _db = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

            if (!_db.Users.Any())
            {
                var post = _db.PostTypes.FirstOrDefault(pt => pt.Name == "admin");
                if (post == null)
                {
                    post = new PostType
                    {
                        Name = "admin",
                    };
                    _db.PostTypes.Add(post);
                    _db.SaveChanges();
                }

                _db.Users.Add(new User
                {
                    Decommissioned = false,
                    Username = "admin",
                    HashPassword = Sha256("admin"),
                    Firstname = "",
                    Lastname = "",
                    Surname = "",
                    PostTypeId = post.Id
                });
                _db.SaveChanges();
            }
        }

        public async Task<bool> CheckSession(HttpRequest request)
        {
            if (!request.Cookies.TryGetValue("session", out var key))
                return false;

            return await _db.Sessions.AnyAsync(s =>
                s.Key == key &&
                s.EndDate > DateTime.UtcNow
            );
        }

        
        public async Task<User?> GetUserBySession(HttpRequest request)
        {
            if (!request.Cookies.TryGetValue("session", out var key))
                return null;

            return await _db.Users.Join(
                _db.Sessions,
                u => u.Id,
                s => s.UserId,
                (u, s) => u
            ).FirstOrDefaultAsync();
        }


        private class AuthRequest
        {
            public string Username { get; set; } = default!;
            public string Password { get; set; } = default!;
        }

        private async Task<IResult> LoginApi([FromBody] AuthRequest data, HttpResponse response)
        {
            var user = await _db.Users
                .Where(u => u.Username == data.Username)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.HashPassword,
                }).FirstOrDefaultAsync();

            if (user == null || user.HashPassword != Sha256(data.Password))
                return Results.Unauthorized();

            var ses = new Session
            {
                Key = GenerateRandomSha256Key(),
                UserId = user.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(24),
            };
            _db.Sessions.Add(ses);
            await _db.SaveChangesAsync();

            response.Cookies.Append("session", ses.Key, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = ses.EndDate
            });

            return Results.Ok(new { key = ses.Key });
        }


        private async Task<IResult> CheckSessionApi(HttpRequest request)
        {
            var res = await CheckSession(request);
            return Results.Ok(new { is_session = res });
        }
        private async Task<IResult> GetUserBySessionApi(HttpRequest request)
        {
            var res = await GetUserBySession(request);
            return res == null ? Results.Unauthorized() : Results.Ok(res);
        }

        public static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }

        public static string GenerateRandomSha256Key()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Sha256(Convert.ToHexString(bytes));
        }
    }
}