using System.ComponentModel.DataAnnotations;

namespace valkyrie.Models.Companies
{
	public class Company
	{
		// id (PK) : INTEGER
		[Key]
		public int Id { get; set; }

		// name: VARCHAR(75)
		[Required]
		[MaxLength(75)]
		public string Name { get; set; } = String.Empty;
	}
}
