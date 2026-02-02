using System.ComponentModel.DataAnnotations;

namespace valkyrie.Models.Cars
{
	public class CarType
	{
		// id (PK) : INTEGER
		[Key]
		public int Id { get; set; }

		// name: VARCHAR(50)
		[Required]
		[MaxLength(50)]
		public string Name { get; set; } = String.Empty;
	}
}
