using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using valkyrie.Models.Companies;

namespace valkyrie.Models.Users
{
	[PrimaryKey(nameof(UserId), nameof(PlatformId))]
	public class UserPlatform
	{
		// platform_id (PK|FK) : INTEGER
		[Column("platform_id")]
		public int PlatformId { get; set; }

		[ForeignKey(nameof(PlatformId))]
		public Platform Platform { get; set; } = null!;

		// user_id (PK|FK) : INTEGER
		[Column("user_id")]
		public int UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public User User { get; set; } = null!;
	}
}
