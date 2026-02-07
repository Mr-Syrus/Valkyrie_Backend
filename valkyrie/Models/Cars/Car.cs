using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using valkyrie.Models.Companies;


namespace valkyrie.Models.Cars
{
	public class Car
	{
		// id (PK) : INTEGER
		[Key]
		[Column("id")]
		public int Id { get; set; }

		// start_date_operation : DATE (REQUIRED)
		[Required]
		[Column("start_date_operation")]
		public DateTime StartDateOperation { get; set; }

		// end_date_operation : DATE (OPTIONAL)
		[Column("end_date_operation")]
		public DateTime? EndDateOperation { get; set; }

		// model_car_id (FK) : INTEGER (REQUIRED)
		[Required]
		[Column("model_car_id")]
		public int ModelCarId { get; set; }

		[ForeignKey(nameof(ModelCarId))]
		public ModelCar ModelCar { get; set; } = null!;

		// platform_id (FK) : INTEGER (REQUIRED)
		[Required]
		[Column("platform_id")]
		public int PlatformId { get; set; }

		[ForeignKey(nameof(PlatformId))]
		public Platform Platform { get; set; } = null!;

		// decommissioned : BOOLEAN (REQUIRED)
		[Required] [Column("decommissioned")] public bool Decommissioned { get; set; }

        // number : VARCHAR(11) (REQUIRED)
        [Required]
        [MaxLength(11)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;
    }
}