-- Migration: Add SectorId column to Clients table and drop Sector string column
-- Date: 2025-12-04
-- Description: Convert Sector from string to foreign key relationship

USE APGDb;
GO

-- Step 1: Add SectorId column (nullable initially to allow data migration)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND name = 'SectorId')
BEGIN
    ALTER TABLE [dbo].[Clients]
    ADD [SectorId] INT NULL;
    
    PRINT 'Added SectorId column to Clients table';
END
ELSE
BEGIN
    PRINT 'SectorId column already exists';
END
GO

-- Step 2: Try to migrate existing data
-- Find matching sectors by name or create new ones if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND name = 'Sector')
BEGIN
    -- For each unique sector name in Clients, ensure a Sector entity exists
    DECLARE @SectorName NVARCHAR(200);
    DECLARE @SectorId INT;
    
    DECLARE sector_cursor CURSOR FOR
    SELECT DISTINCT [Sector] FROM [dbo].[Clients] WHERE [Sector] IS NOT NULL AND [Sector] != '';
    
    OPEN sector_cursor;
    FETCH NEXT FROM sector_cursor INTO @SectorName;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Check if sector exists
        SELECT @SectorId = Id FROM [dbo].[Sectors] WHERE [Name] = @SectorName AND [IsActive] = 1;
        
        -- If sector doesn't exist, create it
        IF @SectorId IS NULL
        BEGIN
            INSERT INTO [dbo].[Sectors] ([Name], [IsActive], [BusinessUnitId], [CreatedAt])
            VALUES (@SectorName, 1, NULL, GETUTCDATE());
            
            SET @SectorId = SCOPE_IDENTITY();
            PRINT 'Created sector: ' + @SectorName + ' with ID: ' + CAST(@SectorId AS VARCHAR(10));
        END
        
        -- Update clients with this sector name
        UPDATE [dbo].[Clients]
        SET [SectorId] = @SectorId
        WHERE [Sector] = @SectorName AND [SectorId] IS NULL;
        
        FETCH NEXT FROM sector_cursor INTO @SectorName;
    END
    
    CLOSE sector_cursor;
    DEALLOCATE sector_cursor;
    
    PRINT 'Migrated existing sector data';
END
GO

-- Step 3: Set SectorId to NOT NULL (after data migration)
IF EXISTS (SELECT * FROM sys.columns 
           WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') 
           AND name = 'SectorId' 
           AND is_nullable = 1)
BEGIN
    -- First, handle any remaining NULL values with a default sector
    IF EXISTS (SELECT 1 FROM [dbo].[Clients] WHERE [SectorId] IS NULL)
    BEGIN
        DECLARE @DefaultSectorId INT;
        
        -- Get or create a default "Unknown" sector
        SELECT @DefaultSectorId = Id FROM [dbo].[Sectors] WHERE [Name] = 'Unknown' AND [IsActive] = 1;
        
        IF @DefaultSectorId IS NULL
        BEGIN
            INSERT INTO [dbo].[Sectors] ([Name], [IsActive], [BusinessUnitId], [CreatedAt])
            VALUES ('Unknown', 1, NULL, GETUTCDATE());
            
            SET @DefaultSectorId = SCOPE_IDENTITY();
            PRINT 'Created default Unknown sector with ID: ' + CAST(@DefaultSectorId AS VARCHAR(10));
        END
        
        UPDATE [dbo].[Clients]
        SET [SectorId] = @DefaultSectorId
        WHERE [SectorId] IS NULL;
        
        PRINT 'Set default sector for clients with NULL SectorId';
    END
    
    -- Now make SectorId NOT NULL
    ALTER TABLE [dbo].[Clients]
    ALTER COLUMN [SectorId] INT NOT NULL;
    
    PRINT 'Made SectorId column NOT NULL';
END
ELSE
BEGIN
    PRINT 'SectorId column is already NOT NULL';
END
GO

-- Step 4: Add foreign key constraint
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Clients_Sectors')
BEGIN
    ALTER TABLE [dbo].[Clients]
    ADD CONSTRAINT FK_Clients_Sectors
    FOREIGN KEY ([SectorId]) REFERENCES [dbo].[Sectors]([Id]);
    
    PRINT 'Added foreign key constraint FK_Clients_Sectors';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint already exists';
END
GO

-- Step 5: Drop old Sector column
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND name = 'Sector')
BEGIN
    ALTER TABLE [dbo].[Clients]
    DROP COLUMN [Sector];
    
    PRINT 'Dropped old Sector string column';
END
ELSE
BEGIN
    PRINT 'Sector column already dropped';
END
GO

PRINT 'Migration 003_AddSectorIdToClients completed successfully';
GO
