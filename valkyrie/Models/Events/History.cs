using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using valkyrie.Models.Users;


namespace valkyrie.Models.Events
{
	public class History
	{
		// id (PK) : INTEGER
		[Key]
		[Column("id")]
		public int Id { get; set; }

		// event_id (FK) : INTEGER
		[Required]
		[Column("event_id")]
		public int EventId { get; set; }

		[ForeignKey(nameof(EventId))]
		public Event Event { get; set; } = null!;

		// answer : BOOLEAN
		[Required]
		[Column("answer")]
		public bool Answer { get; set; }

		// data_time : TIMESTAMP WITH TIME ZONE
		[Required]
		[Column("data_time")]
		public DateTimeOffset DataTime { get; set; }

		// user_id (FK) : INTEGER
		[Required]
		[Column("user_id")]
		public int UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public User User { get; set; } = null!;
	}
}
