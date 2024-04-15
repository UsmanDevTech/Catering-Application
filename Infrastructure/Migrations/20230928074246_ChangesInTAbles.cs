using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ChangesInTAbles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrgMealCatAndOrgMealitems_Items_ItemsId",
                table: "OrgMealCatAndOrgMealitems");

            migrationBuilder.DropIndex(
                name: "IX_OrgMealCatAndOrgMealitems_ItemsId",
                table: "OrgMealCatAndOrgMealitems");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "OrgMeals");

            migrationBuilder.DropColumn(
                name: "PictureUrl",
                table: "OrgMeals");

            migrationBuilder.DropColumn(
                name: "ItemsId",
                table: "OrgMealCatAndOrgMealitems");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Star",
                table: "OrderRating",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "Items",
                table: "CartItems",
                newName: "ItemName");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "CartItems",
                newName: "CategoryName");

            migrationBuilder.AlterColumn<string>(
                name: "TimeSlot",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MealType",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_OrgMealCatAndOrgMealitems_ItemId",
                table: "OrgMealCatAndOrgMealitems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRating_OrderId",
                table: "OrderRating",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderRating_Orders_OrderId",
                table: "OrderRating",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrgMealCatAndOrgMealitems_Items_ItemId",
                table: "OrgMealCatAndOrgMealitems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderRating_Orders_OrderId",
                table: "OrderRating");

            migrationBuilder.DropForeignKey(
                name: "FK_OrgMealCatAndOrgMealitems_Items_ItemId",
                table: "OrgMealCatAndOrgMealitems");

            migrationBuilder.DropIndex(
                name: "IX_OrgMealCatAndOrgMealitems_ItemId",
                table: "OrgMealCatAndOrgMealitems");

            migrationBuilder.DropIndex(
                name: "IX_OrderRating_OrderId",
                table: "OrderRating");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MealType");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "OrderRating",
                newName: "Star");

            migrationBuilder.RenameColumn(
                name: "ItemName",
                table: "CartItems",
                newName: "Items");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "CartItems",
                newName: "Category");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "OrgMeals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PictureUrl",
                table: "OrgMeals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemsId",
                table: "OrgMealCatAndOrgMealitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeSlot",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OrgMealCatAndOrgMealitems_ItemsId",
                table: "OrgMealCatAndOrgMealitems",
                column: "ItemsId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrgMealCatAndOrgMealitems_Items_ItemsId",
                table: "OrgMealCatAndOrgMealitems",
                column: "ItemsId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
