using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace valkyrie.Models.Events
{
	public class TypeEvent
	{
		// id (PK) : INTEGER
		[Key]
		[Column("id")]
		public int Id { get; set; }

		// name: VARCHAR(20)
		[Required]
		[MaxLength(20)]
		[Column("name")]
		public string Name { get; set; } = String.Empty;
	}
}
