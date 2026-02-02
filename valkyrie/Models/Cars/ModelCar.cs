using Microsoft.VisualBasic.FileIO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace valkyrie.Models.Cars
{
	public class ModelCar
	{
		// id (PK) : INTEGER
		[Key]
		public int Id { get; set; }

		// fuer_tipe : ENUM ('petrol', 'diesel', 'gas', 'hydrogen', 'electricity')
		[Required]
		public FuelType FuelType { get; set; }

		// car_brand_id (PK) : INTEGER
		[Required]
		public int CarBrandId { get; set; }
		[ForeignKey(nameof(CarBrandId))]
		public CarBrand CarBrand { get; set; } = null!;

		// year_release : INTEGER 
		[Required]
		public int YearRelease { get; set; }

		// car_type_id (PK) : INTEGER
		[Required]
		public int CarTypeId { get; set; }
		[ForeignKey(nameof(CarTypeId))]
		public CarType CarType { get; set; } = null!;
	}
}
