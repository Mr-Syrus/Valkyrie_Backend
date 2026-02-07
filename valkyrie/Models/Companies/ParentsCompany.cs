using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using valkyrie.Models.Users;

namespace valkyrie.Models.Companies
{

	public class ParentsCompany
	{
		// id (PK|FK) : INTEGER
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[ForeignKey(nameof(Id))]
		public Company CompanyChild { get; set; } = null!;

		// company_id (FK) : INTEGER
		[Required]
		[Column("company_id")]
		public int CompanyId { get; set; }

		[ForeignKey(nameof(CompanyId))]
		public Company CompanyParents { get; set; } = null!; // родитель
	}
}
