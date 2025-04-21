using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SrgInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuralUnitAndEnrollmentYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnrollmentYear",
                table: "Members",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StructuralUnit",
                table: "Members",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EnrollmentYear",
                table: "Managers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StructuralUnit",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnrollmentYear",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "StructuralUnit",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "EnrollmentYear",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "StructuralUnit",
                table: "Managers");
        }
    }
}
