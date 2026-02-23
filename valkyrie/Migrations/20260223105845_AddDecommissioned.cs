using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace valkyrie.Migrations
{
    /// <inheritdoc />
    public partial class AddDecommissioned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "decommissioned",
                table: "Companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "decommissioned",
                table: "Companies");
        }
    }
}
