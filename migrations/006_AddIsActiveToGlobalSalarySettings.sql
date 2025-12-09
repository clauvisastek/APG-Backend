-- Add IsActive column to GlobalSalarySettings
-- Created: 2024-12-05

-- Add IsActive column with default value false
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[GlobalSalarySettings]') 
    AND name = 'IsActive'
)
BEGIN
    ALTER TABLE [dbo].[GlobalSalarySettings]
    ADD [IsActive] BIT NOT NULL DEFAULT 0;
    
    PRINT 'Column IsActive added to GlobalSalarySettings';
END
GO

-- Create index on IsActive for performance
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE object_id = OBJECT_ID(N'[dbo].[GlobalSalarySettings]') 
    AND name = 'IX_GlobalSalarySettings_IsActive'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_GlobalSalarySettings_IsActive] 
        ON [dbo].[GlobalSalarySettings]([IsActive] ASC);
    
    PRINT 'Index IX_GlobalSalarySettings_IsActive created';
END
GO

-- If there are existing records, activate the most recent one
DECLARE @RecordCount INT;
SELECT @RecordCount = COUNT(*) FROM [dbo].[GlobalSalarySettings];

IF @RecordCount > 0
BEGIN
    -- Set the most recent record as active
    UPDATE [dbo].[GlobalSalarySettings]
    SET [IsActive] = 1
    WHERE Id = (
        SELECT TOP 1 Id 
        FROM [dbo].[GlobalSalarySettings] 
        ORDER BY CreatedAt DESC
    );
    
    PRINT 'Most recent GlobalSalarySettings record set as active';
END
GO
