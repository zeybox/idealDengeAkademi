-- Courses tablosuna "Bu eğitimde neler var?" ve "İçerik özeti" alanları.
-- Bu script'i SQL Server Management Studio veya Azure Data Studio'da çalıştırın.

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Courses')
BEGIN
  IF NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Courses' AND c.name = 'Highlights')
    ALTER TABLE Courses ADD Highlights nvarchar(2000) NULL;
  IF NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Courses' AND c.name = 'ContentSummary')
    ALTER TABLE Courses ADD ContentSummary nvarchar(1000) NULL;
END
GO
