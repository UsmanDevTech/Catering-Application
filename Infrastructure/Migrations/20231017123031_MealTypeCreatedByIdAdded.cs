using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class MealTypeCreatedByIdAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "MealType",
                newName: "CreatedByType");

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "MealType",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "MealType");

            migrationBuilder.RenameColumn(
                name: "CreatedByType",
                table: "MealType",
                newName: "CreatedBy");
        }
    }
}
