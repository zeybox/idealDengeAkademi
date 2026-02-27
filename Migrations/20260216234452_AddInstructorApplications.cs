using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HizliOgren.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'InstructorApplications')
BEGIN
    CREATE TABLE InstructorApplications (
        Id int IDENTITY(1,1) PRIMARY KEY,
        FullName nvarchar(200) NOT NULL,
        Email nvarchar(256) NOT NULL,
        Phone nvarchar(50) NULL,
        City nvarchar(100) NULL,
        Message nvarchar(2000) NULL,
        UserId int NULL,
        Status int NOT NULL DEFAULT 0,
        AdminNotes nvarchar(2000) NULL,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_InstructorApplications_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
    );
    CREATE INDEX IX_InstructorApplications_UserId ON InstructorApplications(UserId);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = N'InstructorApplications')
    DROP TABLE InstructorApplications;
");
        }
    }
}
