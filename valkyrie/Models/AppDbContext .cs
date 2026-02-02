using Microsoft.EntityFrameworkCore;
using valkyrie.Models.Cars;
using valkyrie.Models.Companies;
using valkyrie.Models.Events;
using valkyrie.Models.Users;

namespace valkyrie.Models
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options)
			: base(options)
		{
		}

		// Users
		public DbSet<Car> Cars { get; set; }
		public DbSet<CarBrand> CarBrands { get; set; }
		public DbSet<CarType> CarTypes { get; set; }
		public DbSet<ModelCar> ModelCars { get; set; }

		// Companies
		public DbSet<Company> Companies { get; set; }
		public DbSet<ParentsCompany> ParentsCompanies { get; set; }
		public DbSet<Platform> Platforms { get; set; }

		// Events
		public DbSet<Event> Events { get; set; }
		public DbSet<History> Histories { get; set; }
		public DbSet<TypeEvent> TypeEvents { get; set; }

		// Users
		public DbSet<PostType> PostTypes { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<UserCompany> UserCompanies { get; set; }
		public DbSet<UserPlatform> UserPlatforms { get; set; }

	}
}
