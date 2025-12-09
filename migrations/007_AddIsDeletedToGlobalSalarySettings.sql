-- Migration: 007_AddIsDeletedToGlobalSalarySettings
-- Description: Add IsDeleted column to GlobalSalarySettings table for soft delete functionality
-- Date: 2025-12-05

BEGIN TRANSACTION;

-- Add IsDeleted column with default value FALSE (0)
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[GlobalSalarySettings]') 
    AND name = 'IsDeleted'
)
BEGIN
    ALTER TABLE [dbo].[GlobalSalarySettings]
    ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
    
    PRINT 'Added IsDeleted column to GlobalSalarySettings table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in GlobalSalarySettings table';
END

-- Create index on IsDeleted for query filter performance
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_GlobalSalarySettings_IsDeleted' 
    AND object_id = OBJECT_ID(N'[dbo].[GlobalSalarySettings]')
)
BEGIN
    CREATE INDEX [IX_GlobalSalarySettings_IsDeleted] 
    ON [dbo].[GlobalSalarySettings] ([IsDeleted]);
    
    PRINT 'Created index IX_GlobalSalarySettings_IsDeleted';
END
ELSE
BEGIN
    PRINT 'Index IX_GlobalSalarySettings_IsDeleted already exists';
END

COMMIT TRANSACTION;

PRINT 'Migration 007_AddIsDeletedToGlobalSalarySettings completed successfully';
GO
