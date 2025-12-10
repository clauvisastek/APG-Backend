-- Migration: AddResourceHistory
-- Description: Create ResourceHistories table to track all resource changes over time
-- This enables tracking salary increases, rate changes, and their impact on project margins

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ResourceHistories')
BEGIN
    CREATE TABLE ResourceHistories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ResourceId INT NOT NULL,
        ChangeType NVARCHAR(50) NOT NULL,
        ChangeDescription NVARCHAR(500) NOT NULL,
        OldValuesJson NVARCHAR(4000) NULL,
        NewValuesJson NVARCHAR(4000) NULL,
        OldDailyCostRate DECIMAL(18,2) NULL,
        NewDailyCostRate DECIMAL(18,2) NULL,
        OldDailySellRate DECIMAL(18,2) NULL,
        NewDailySellRate DECIMAL(18,2) NULL,
        OldMarginRate DECIMAL(5,2) NULL,
        NewMarginRate DECIMAL(5,2) NULL,
        ImpactNotes NVARCHAR(1000) NULL,
        ChangedBy NVARCHAR(200) NULL,
        ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_ResourceHistories_Resources FOREIGN KEY (ResourceId) 
            REFERENCES Resources(Id) ON DELETE CASCADE
    );

    -- Indexes for efficient querying
    CREATE INDEX IX_ResourceHistories_ResourceId ON ResourceHistories(ResourceId);
    CREATE INDEX IX_ResourceHistories_ChangedAt ON ResourceHistories(ChangedAt);
    CREATE INDEX IX_ResourceHistories_ChangeType ON ResourceHistories(ChangeType);

    PRINT 'ResourceHistories table created successfully';
END
ELSE
BEGIN
    PRINT 'ResourceHistories table already exists';
END
GO
