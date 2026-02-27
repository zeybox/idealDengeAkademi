-- Run this script on the database your app uses if the migration cannot be applied
-- (e.g. when the app uses a different connection than dotnet ef).
-- Adds PaidContent column to Courses if it does not exist.

IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'Courses' AND c.name = 'PaidContent'
)
BEGIN
    ALTER TABLE Courses ADD PaidContent nvarchar(max) NULL;
END
GO
