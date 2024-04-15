using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class IDnameChanged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserOrderId",
                table: "UserOrders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OtpId",
                table: "OTPhistories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OrgMealId",
                table: "OrgMeals",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OrgMealCategoryId",
                table: "OrgMealCategories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Organizations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Orders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OrderItemId",
                table: "OrderItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "MealTypeId",
                table: "MealType",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Items",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CartId",
                table: "Carts",
                newName: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserOrders",
                newName: "UserOrderId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OTPhistories",
                newName: "OtpId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OrgMeals",
                newName: "OrgMealId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OrgMealCategories",
                newName: "OrgMealCategoryId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Organizations",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Orders",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OrderItems",
                newName: "OrderItemId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "MealType",
                newName: "MealTypeId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Items",
                newName: "ItemId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Carts",
                newName: "CartId");
        }
    }
}
