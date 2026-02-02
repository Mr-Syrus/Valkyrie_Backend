using System.ComponentModel.DataAnnotations;

namespace valkyrie.Models.Events
{
	public class TypeEvent
	{

		// id (PK) : INTEGER
		[Key]
		public int Id { get; set; }

		// name: VARCHAR(20)
		[Required]
		[MaxLength(20)]
		public string Name { get; set; } = String.Empty;
	}
}
