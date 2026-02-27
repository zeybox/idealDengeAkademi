-- Eğitmen profil alanları: Users tablosuna eklenir.
-- Bu script'i SQL Server Management Studio veya dotnet ef database update ile çalıştırabilirsiniz.
-- Zaten kolon varsa hata vermez (IF NOT EXISTS kullanıldı).

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
GO
