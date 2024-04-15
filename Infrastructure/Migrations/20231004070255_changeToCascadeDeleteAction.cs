using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class changeToCascadeDeleteAction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestForSoftDelete",
                table: "AspNetUsers",
                newName: "RequestForPermanenttDelete");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestForPermanenttDelete",
                table: "AspNetUsers",
                newName: "RequestForSoftDelete");
        }
    }
}
