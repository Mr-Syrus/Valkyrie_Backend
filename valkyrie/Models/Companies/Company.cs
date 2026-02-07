using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace valkyrie.Models.Companies
{
	public class Company
	{
		// id (PK) : INTEGER
		[Key]
		[Column("id")]
		public int Id { get; set; }

		// name: VARCHAR(75)
		[Required]
		[MaxLength(75)]
		[Column("name")]
		public string Name { get; set; } = String.Empty;
	}
}
