using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HizliOgren.Migrations
{
    /// <inheritdoc />
    public partial class EnsurePaidContentColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Courses')
AND NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Courses' AND c.name = 'PaidContent')
BEGIN
    ALTER TABLE Courses ADD PaidContent nvarchar(max) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Courses' AND c.name = 'PaidContent')
BEGIN
    ALTER TABLE Courses DROP COLUMN PaidContent;
END
");
        }
    }
}
