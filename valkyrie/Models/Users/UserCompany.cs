using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using valkyrie.Models.Companies;

namespace valkyrie.Models.Users
{
	[PrimaryKey(nameof(UserId), nameof(CompanyId))]
	public class UserCompany
	{
		// company_id (PK | FK) : INTEGER
		[Column("company_id")]
		public int CompanyId { get; set; }

		[ForeignKey(nameof(CompanyId))]
		public Company Company { get; set; } = null!;

		// user_id (PK | FK) : INTEGER
		[Column("user_id")]
		public int UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public User User { get; set; } = null!;
	}
}
