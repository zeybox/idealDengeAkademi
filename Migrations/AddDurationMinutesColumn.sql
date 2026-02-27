-- CurriculumItems tablosuna DurationMinutes kolonunu ekler.
-- Bu script'i SQL Server'da (SSMS veya sqlcmd) çalıştırın.
-- Migration'ı EF geçmişine eklemek için ikinci komutu da çalıştırın.

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.CurriculumItems') AND name = 'DurationMinutes'
)
BEGIN
    ALTER TABLE [CurriculumItems] ADD [DurationMinutes] int NULL;
END
GO

-- EF migration geçmişine ekle (sadece kolon yokken çalıştırdıysanız):
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260202000001_AddDurationMinutesToCurriculumItem')
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260202000001_AddDurationMinutesToCurriculumItem', N'10.0.0');
GO
