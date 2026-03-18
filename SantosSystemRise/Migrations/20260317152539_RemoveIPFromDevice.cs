using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SantosSystemRise.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIPFromDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IP",
                table: "Devices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IP",
                table: "Devices",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
