using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HizliOgren.Migrations
{
    [Migration("20260217000001_AddInstructorProfileToUser")]
    public partial class AddInstructorProfileToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Users' AND c.name = 'Title')
        ALTER TABLE Users ADD Title nvarchar(150) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Users' AND c.name = 'Bio')
        ALTER TABLE Users ADD Bio nvarchar(2000) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Users' AND c.name = 'AvatarUrl')
        ALTER TABLE Users ADD AvatarUrl nvarchar(500) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Users' AND c.name = 'Phone')
        ALTER TABLE Users ADD Phone nvarchar(50) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Users' AND c.name = 'Title')
        ALTER TABLE Users DROP COLUMN Title;
    IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Users' AND c.name = 'Bio')
        ALTER TABLE Users DROP COLUMN Bio;
    IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Users' AND c.name = 'AvatarUrl')
        ALTER TABLE Users DROP COLUMN AvatarUrl;
    IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Users' AND c.name = 'Phone')
        ALTER TABLE Users DROP COLUMN Phone;
END
");
        }
    }
}
