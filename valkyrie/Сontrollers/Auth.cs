using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using valkyrie.Models;
using valkyrie.Models.Users;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace valkyrie.Сontrollers
{
	public class Auth
	{
		private readonly AppDbContext _db;

		public Auth(WebApplication app, RouteGroupBuilder router) 
		{
			var routerAuth = router.MapGroup("/auth");
			
			routerAuth.MapPost("/login", LoginApi);
			routerAuth.MapGet("/check-session", LoginApi);
			
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
					Firstname="",
					Lastname="",
					Surname="",
					PostTypeId = post.Id
				});
				_db.SaveChanges();
			}
		}

		private string Sha256(string input)
		{
			using var sha = SHA256.Create();
			var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
			return Convert.ToHexString(bytes);
		}
		private string GenerateRandomSha256Key()
		{
			var bytes = RandomNumberGenerator.GetBytes(32);
			return Convert.ToHexString(bytes).ToLower();
		}

		private class AuthRequest
		{
			public string Username { get; set; } = default!;
			public string Password { get; set; } = default!;
		}

		private async Task<IResult> LoginApi(AuthRequest data, HttpResponse response)
		{
			var user = await _db.Users
				.Where(u => u.Username == data.Username)
				.Select(u => new {
					u.Id,
					u.Username,
					u.HashPassword,
				}).FirstOrDefaultAsync();

			if (user == null || user.HashPassword == Sha256(data.Password))
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

		public async Task<bool> CheckSession(HttpRequest request)
		{
			if (!request.Cookies.TryGetValue("session_key", out var key))
				return false;

			return await _db.Sessions.AnyAsync(s =>
				s.Key == key &&
				s.EndDate > DateTime.UtcNow
			);
		}

		public async Task<IResult> CheckSessionApi(HttpRequest request)
		{
			var res = await CheckSession(request);
			return Results.Ok(new { is_session = res });
		}

	}
}
