using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservation_Management_App.Repository.Migrations
{
    /// <inheritdoc />
    public partial class phoneAddedToLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Locations");
        }
    }
}
