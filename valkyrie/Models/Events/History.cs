using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using valkyrie.Models.Users;


namespace valkyrie.Models.Events
{
	public class History
	{
		// id (PK) : INTEGER
		[Key]
		public int Id { get; set; }

		// event_id (FK) : INTEGER
		[Required]
		public int EventId { get; set; }

		[ForeignKey(nameof(EventId))]
		public Event Event { get; set; } = null!;

		// answer : BOOLEAN
		[Required]
		public bool Answer { get; set; }

		// data_time : TIMESTAMP WITH TIME ZONE
		[Required]
		public DateTimeOffset DataTime { get; set; }

		// user_id (FK) : INTEGER
		[Required]
		public int UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public Users.User User { get; set; } = null!;
	}
}
