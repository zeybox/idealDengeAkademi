-- SSMS veya Azure Data Studio'da veritabanına YETKİLİ bir hesapla (sa / db_owner) bağlanıp çalıştırın.
-- hizliogrennet kullanıcısının ALTER yetkisi yok; bu yüzden bu script'i yetkili hesapla çalıştırmanız gerekiyor.

-- 1) Kolonu ekle
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = N'CurriculumItems' AND c.name = N'DurationMinutes'
)
BEGIN
    ALTER TABLE [CurriculumItems] ADD [DurationMinutes] int NULL;
END
GO

-- 2) EF migration geçmişine ekle (dotnet ef tekrar uygulamaya çalışmasın diye)
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260202000001_AddDurationMinutesToCurriculumItem')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260202000001_AddDurationMinutesToCurriculumItem', N'10.0.0');
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260201234202_EnsureDurationMinutesColumn')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260201234202_EnsureDurationMinutesColumn', N'10.0.0');
GO
