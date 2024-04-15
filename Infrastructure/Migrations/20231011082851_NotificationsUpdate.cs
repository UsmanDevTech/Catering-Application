using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class NotificationsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MealId",
                table: "Notifications",
                newName: "NotifiedToId");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Notifications",
                newName: "NotifiedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NotifiedToId",
                table: "Notifications",
                newName: "MealId");

            migrationBuilder.RenameColumn(
                name: "NotifiedById",
                table: "Notifications",
                newName: "ItemId");
        }
    }
}
