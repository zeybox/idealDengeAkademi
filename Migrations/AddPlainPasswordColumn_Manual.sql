-- PlainPassword kolonunu ekle (dotnet ef migration zincirinde olmadığı için manuel çalıştırın)
-- Veritabanı: hizliogrennet (veya kullandığınız DB adı)

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Users' AND c.name = 'PlainPassword')
        ALTER TABLE Users ADD PlainPassword nvarchar(256) NULL;
END
GO

-- Migration'ı uygulandı olarak işaretle (EF Core bir daha uygulamaya çalışmasın diye)
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260302000001_AddPlainPasswordToUsers')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260302000001_AddPlainPasswordToUsers', N'10.0.0');
GO
