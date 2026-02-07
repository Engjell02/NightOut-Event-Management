using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservation_Management_App.Repository.Migrations
{
    /// <inheritdoc />
    public partial class migrationApiImported : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ImportedFromApi",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportedFromApi",
                table: "Events");
        }
    }
}
