-- Migration: 009_AddProjectsTable
-- Description: Create Projects table for project management
-- Created: 2025-12-09

-- Create Projects table
CREATE TABLE Projects (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    ClientId INT NOT NULL,
    BusinessUnitId INT NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    ResponsibleName NVARCHAR(200) NULL,
    Currency NVARCHAR(3) NOT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    TargetMargin DECIMAL(5,2) NOT NULL,
    MinMargin DECIMAL(5,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'En construction',
    Notes NVARCHAR(2000) NULL,
    YtdRevenue DECIMAL(18,2) NULL,
    TeamMembersJson NVARCHAR(MAX) NULL,
    GlobalMarginHistoryJson NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    -- Foreign keys
    CONSTRAINT FK_Projects_Clients FOREIGN KEY (ClientId) REFERENCES Clients(Id),
    CONSTRAINT FK_Projects_BusinessUnits FOREIGN KEY (BusinessUnitId) REFERENCES BusinessUnits(Id),
    
    -- Unique constraint on Code
    CONSTRAINT UQ_Projects_Code UNIQUE (Code)
);

-- Create indexes for better query performance
CREATE INDEX IX_Projects_Name ON Projects(Name);
CREATE INDEX IX_Projects_ClientId ON Projects(ClientId);
CREATE INDEX IX_Projects_BusinessUnitId ON Projects(BusinessUnitId);
CREATE INDEX IX_Projects_Status ON Projects(Status);
CREATE INDEX IX_Projects_StartDate ON Projects(StartDate);
CREATE INDEX IX_Projects_IsActive ON Projects(IsActive);

PRINT 'Migration 009_AddProjectsTable completed successfully';
