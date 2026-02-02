using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace valkyrie.Models.Companies
{
	public class Platform
	{
		// id (PK) : INTEGER
		[Key]
		public int Id { get; set; }

		// name : VARCHAR(75) (REQUIRED)
		[Required]
		[MaxLength(75)]
		public string Name { get; set; } = string.Empty;

		// address : VARCHAR(75) (REQUIRED)
		[Required]
		[MaxLength(75)]
		public string Address { get; set; } = string.Empty;

		// start_date : DATE (REQUIRED)
		[Required]
		public DateTime StartDate { get; set; }

		// end_date : DATE (OPTIONAL)
		public DateTime? EndDate { get; set; }

		// company_id (FK) : INTEGER (REQUIRED)
		[Required]
		public int CompanyId { get; set; }

		[ForeignKey(nameof(CompanyId))]
		public Company Company { get; set; } = null!;
	}
}
