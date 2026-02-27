using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HizliOgren.Migrations
{
    /// <inheritdoc />
    [Migration("20260216000001_AddPaidContentToCourse")]
    public partial class AddPaidContentToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaidContent",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidContent",
                table: "Courses");
        }
    }
}
