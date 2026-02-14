using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace valkyrie.Models.Companies
{
	public class Platform
	{
		// id (PK) : INTEGER
		[Key]
		[Column("id")]
		public int Id { get; set; }

		// name : VARCHAR(75) (REQUIRED)
		[Required]
		[MaxLength(75)]
		[Column("name")]
		public string Name { get; set; } = string.Empty;

		// address : VARCHAR(75) (REQUIRED)
		[Required]
		[MaxLength(75)]
		[Column("address")]
		public string Address { get; set; } = string.Empty;

		// start_date : DATE (REQUIRED)
		[Required]
		[Column("start_date")]
		public DateTimeOffset StartDate { get; set; }

		// end_date : DATE (OPTIONAL)
		[Column("end_date")]
		public DateTimeOffset? EndDate { get; set; }

		// company_id (FK) : INTEGER (REQUIRED)
		[Required]
		[Column("company_id")]
		public int CompanyId { get; set; }

		[ForeignKey(nameof(CompanyId))]
		public Company Company { get; set; } = null!;
	}
}
