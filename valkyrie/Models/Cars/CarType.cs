using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace valkyrie.Models.Cars
{
	public class CarType
	{
		// id (PK) : INTEGER
		[Key]
		[Column("id")]
		public int Id { get; set; }

		// name : VARCHAR(50)
		[Required]
		[MaxLength(50)]
		[Column("name")]
		public string Name { get; set; } = string.Empty;
	}
}
