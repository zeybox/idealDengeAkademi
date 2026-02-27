using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HizliOgren.Migrations
{
    /// <inheritdoc />
    [Migration("20260202000001_AddDurationMinutesToCurriculumItem")]
    public partial class AddDurationMinutesToCurriculumItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "CurriculumItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "CurriculumItems");
        }
    }
}
