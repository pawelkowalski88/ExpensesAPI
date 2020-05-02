using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpensesAPI.IdentityProvider.Migrations
{
    public partial class AddedEmailLocalPartToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailLocalPart",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailLocalPart",
                table: "AspNetUsers");
        }
    }
}
