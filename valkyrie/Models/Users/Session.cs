using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace valkyrie.Models.Users
{
	public class Session
	{
		// key (PK) : VARCHAR (64)
		[Key]
		[MaxLength(64)]
		[Column("key")]
		public string Key { get; set; } = string.Empty;

		// user_id (FK) : INTEGER
		[Column("user_id")]
		public int UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public User User { get; set; } = null!;

		// start_date : TIMESTAMP WITH TIME ZONE
		[Column("start_date")]
		public DateTimeOffset StartDate { get; set; }

		// end_date : TIMESTAMP WITH TIME ZONE
		[Column("end_date")]
		public DateTimeOffset EndDate { get; set; }
	}
}
