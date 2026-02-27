using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HizliOgren.Migrations
{
    /// <inheritdoc />
    public partial class EnsureDurationMinutesColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Kolon yoksa ekle (idempotent; migration geçmişi senkron değilse bile güvenli)
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = N'CurriculumItems' AND c.name = N'DurationMinutes'
)
BEGIN
    ALTER TABLE [CurriculumItems] ADD [DurationMinutes] int NULL;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
