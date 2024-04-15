using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ChangesInFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Organizations_OrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrganizationCategoryId",
                table: "OrgMealCatAndOrgMealitems");

            migrationBuilder.DropColumn(
                name: "MealCategoryId",
                table: "MealType");

            migrationBuilder.DropColumn(
                name: "WeekDay",
                table: "MealType");

            migrationBuilder.DropColumn(
                name: "NotificationId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrgMealId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "oTPmediaType",
                table: "OTPhistories",
                newName: "OTPmediaTypes");

            migrationBuilder.RenameColumn(
                name: "OrgPicturePath",
                table: "OrgMeals",
                newName: "PictureUrl");

            migrationBuilder.RenameColumn(
                name: "OrgName",
                table: "OrgMeals",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "MealStartTime",
                table: "OrgMeals",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "MealEndTime",
                table: "OrgMeals",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "OrgMealCategories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Organizations",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "CompanyNumber",
                table: "Organizations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "Organizations",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "CompanyEmail",
                table: "Organizations",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "SubCategoryName",
                table: "OrderItems",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ItemName",
                table: "OrderItems",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "PicturePath",
                table: "MealType",
                newName: "PictureUrl");

            migrationBuilder.RenameColumn(
                name: "MealStartTime",
                table: "MealType",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "MealName",
                table: "MealType",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "MealEndTime",
                table: "MealType",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "MealCategories",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "MealCategories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ItemName",
                table: "Items",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "SubCategory",
                table: "CartItems",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "Alergy",
                table: "Allergies",
                newName: "Name");

            migrationBuilder.AlterColumn<int>(
                name: "Employees",
                table: "Organizations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                table: "MealType",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OTPmediaTypes",
                table: "OTPhistories",
                newName: "oTPmediaType");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "OrgMeals",
                newName: "MealStartTime");

            migrationBuilder.RenameColumn(
                name: "PictureUrl",
                table: "OrgMeals",
                newName: "OrgPicturePath");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "OrgMeals",
                newName: "OrgName");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "OrgMeals",
                newName: "MealEndTime");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "OrgMealCategories",
                newName: "CategoryName");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Organizations",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Organizations",
                newName: "CompanyNumber");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Organizations",
                newName: "CompanyName");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Organizations",
                newName: "CompanyEmail");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "OrderItems",
                newName: "SubCategoryName");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "OrderItems",
                newName: "ItemName");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "MealType",
                newName: "MealStartTime");

            migrationBuilder.RenameColumn(
                name: "PictureUrl",
                table: "MealType",
                newName: "PicturePath");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "MealType",
                newName: "MealName");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "MealType",
                newName: "MealEndTime");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "MealCategories",
                newName: "CategoryName");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "MealCategories",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Items",
                newName: "ItemName");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "CartItems",
                newName: "SubCategory");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Allergies",
                newName: "Alergy");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationCategoryId",
                table: "OrgMealCatAndOrgMealitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Employees",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "MealType",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "MealCategoryId",
                table: "MealType",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WeekDay",
                table: "MealType",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NotificationId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrgMealId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organizations_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
