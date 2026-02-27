-- DurationMinutes kolonu veritabanında yok ama EF "already up to date" diyorsa:
-- 1) Bu script'i SSMS veya Azure Data Studio'da veritabanınıza bağlanıp çalıştırın.
-- 2) Sonra proje klasöründe: dotnet ef database update

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260202000001_AddDurationMinutesToCurriculumItem';
GO
