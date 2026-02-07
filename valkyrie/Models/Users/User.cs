using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace valkyrie.Models.Users
{
    public class User
    {
        // id (PK) : INTEGER
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // decommissioned : BOOLEAN
        [Required]
        [Column("decommissioned")]
        public bool Decommissioned { get; set; } = false;
		
        // is_admin : BOOLEAN
        [Required]
        [Column("is_admin")]
        public bool IsAdmin { get; set; } = false;

        // username : VARCHAR(25)
        [Required]
        [MaxLength(25)]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        // hash_password : VARCHAR(64)
        [Required]
        [MaxLength(64)]
        [Column("hash_password")]
        public string HashPassword { get; set; } = string.Empty;

        // firstname : VARCHAR(25)
        [Required]
        [MaxLength(25)]
        [Column("firstname")]
        public string Firstname { get; set; } = string.Empty;

        // lastname : VARCHAR(25)
        [Required]
        [MaxLength(25)]
        [Column("lastname")]
        public string Lastname { get; set; } = string.Empty;

        // surname : VARCHAR(25)
        [Required]
        [MaxLength(25)]
        [Column("surname")]
        public string Surname { get; set; } = string.Empty;

        // post_type_id (FK) : INTEGER
        [Required]
        [Column("post_type_id")]
        public int PostTypeId { get; set; }

        [ForeignKey(nameof(PostTypeId))]
        public PostType PostType { get; set; } = null!;
    }
}