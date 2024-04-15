using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class changeUserIdnameinOtpHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "OTPhistories",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "OTPmediaTypes",
                table: "OTPhistories",
                newName: "OTPmediaTyp");

            migrationBuilder.RenameColumn(
                name: "ExpireDate",
                table: "OTPhistories",
                newName: "ExpireDateTime");

            migrationBuilder.AddColumn<string>(
                name: "ReplacementValue",
                table: "OTPhistories",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReplacementValue",
                table: "OTPhistories");

            migrationBuilder.RenameColumn(
                name: "OTPmediaTyp",
                table: "OTPhistories",
                newName: "OTPmediaTypes");

            migrationBuilder.RenameColumn(
                name: "ExpireDateTime",
                table: "OTPhistories",
                newName: "ExpireDate");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "OTPhistories",
                newName: "UserId");
        }
    }
}
