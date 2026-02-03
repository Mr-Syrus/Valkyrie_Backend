using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace valkyrie.Models.Users
{
	public class Session
	{
		// key (PK) : VARCHAR (32)
		[Key]
		[MaxLength(32)]
		public string Key { get; set; } = string.Empty;

		// user_id (FK) : INTEGER
		public int UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public User User { get; set; } = null!;

		// start_date : TIMESTAMP WITH TIME ZONE
		public DateTimeOffset StartDate { get; set; }

		// end_date : TIMESTAMP WITH TIME ZONE
		public DateTimeOffset EndDate { get; set; }
	}
}
