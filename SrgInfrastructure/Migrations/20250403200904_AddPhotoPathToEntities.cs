using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SrgInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoPathToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Members",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Managers",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Departments",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUserViewModel");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Departments");
        }
    }
}
