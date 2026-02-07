using Microsoft.VisualBasic.FileIO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace valkyrie.Models.Cars
{
	public class ModelCar
	{
		// id (PK) : INTEGER
		[Key]
		[Column("id")]
		public int Id { get; set; }

		// fuel_type : ENUM ('petrol', 'diesel', 'gas', 'hydrogen', 'electricity')
		[Required]
		[Column("fuel_type")]
		public FuelType FuelType { get; set; }

		// car_brand_id (FK) : INTEGER
		[Required]
		[Column("car_brand_id")]
		public int CarBrandId { get; set; }

		[ForeignKey(nameof(CarBrandId))]
		public CarBrand CarBrand { get; set; } = null!;

		// year_release : INTEGER 
		[Required]
		[Column("year_release")]
		public int YearRelease { get; set; }

		// car_type_id (FK) : INTEGER
		[Required]
		[Column("car_type_id")]
		public int CarTypeId { get; set; }

		[ForeignKey(nameof(CarTypeId))]
		public CarType CarType { get; set; } = null!;
	}
}
