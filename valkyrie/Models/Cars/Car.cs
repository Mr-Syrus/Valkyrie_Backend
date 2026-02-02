using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using valkyrie.Models.Companies;


namespace valkyrie.Models.Cars
{
	public class Car
	{
		// id (PK) : INTEGER
		[Key]
		public int Id { get; set; }

		// start_date_operation : DATE (REQUIRED)
		[Required]
		public DateTime StartDateOperation { get; set; }

		// end_date_operation : DATE (OPTIONAL)
		public DateTime? EndDateOperation { get; set; }

		// model_cars_id (FK) : INTEGER (REQUIRED)
		[Required]
		public int ModelCarId { get; set; }

		[ForeignKey(nameof(ModelCarId))]
		public ModelCar ModelCar { get; set; } = null!;

		// platform_id (FK) : INTEGER (REQUIRED)
		[Required]
		public int PlatformId { get; set; }

		[ForeignKey(nameof(PlatformId))]
		public Platform Platform { get; set; } = null!;

		// decommissioned : BOOLEAN (REQUIRED)
		[Required]
		public bool Decommissioned { get; set; }

		// number : VARCHAR(11) (REQUIRED)
		[Required]
		[MaxLength(11)]
		public string Number { get; set; } = string.Empty;
	}
}
