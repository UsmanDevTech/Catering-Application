using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class addedOriginalandReplacementValuesToOtpHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OTPmediaTyp",
                table: "OTPhistories",
                newName: "OtpMediaType");

            migrationBuilder.AddColumn<string>(
                name: "OriginalValue",
                table: "OTPhistories",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalValue",
                table: "OTPhistories");

            migrationBuilder.RenameColumn(
                name: "OtpMediaType",
                table: "OTPhistories",
                newName: "OTPmediaTyp");
        }
    }
}
