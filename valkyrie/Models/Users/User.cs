using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace valkyrie.Models.Users
{
	public class User
	{
		// id (PK) : INTEGER
		[Key]
		public int Id { get; set; }

		// decommissioned : BOOLEAN
		[Required]
		public bool Decommissioned { get; set; }

		// username : VARCHAR(25)
		[Required]
		[MaxLength(25)]
		public string Username { get; set; } = string.Empty;

		// hash_password : VARCHAR(64)
		[Required]
		[MaxLength(64)]
		public string HashPassword { get; set; } = string.Empty;

		// firstname : VARCHAR(25)
		[Required]
		[MaxLength(25)]
		public string Firstname { get; set; } = string.Empty;

		// lastname : VARCHAR(25)
		[Required]
		[MaxLength(25)]
		public string Lastname { get; set; } = string.Empty;

		// surname : VARCHAR(25)
		[Required]
		[MaxLength(25)]
		public string Surname { get; set; } = string.Empty;

		// post_type_id (FK) : INTEGER
		[Required]
		public int PostTypeId { get; set; }

		[ForeignKey(nameof(PostTypeId))]
		public PostType PostType { get; set; } = null!;
	}
}
