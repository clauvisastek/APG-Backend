-- Add Calculator Settings Tables
-- Created: 2024-12-05

-- Create GlobalSalarySettings table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GlobalSalarySettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[GlobalSalarySettings] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [EmployerChargesRate] DECIMAL(5,2) NOT NULL,
        [IndirectAnnualCosts] DECIMAL(18,2) NOT NULL,
        [BillableHoursPerYear] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_GlobalSalarySettings] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT 'Table GlobalSalarySettings created';
END
GO

-- Create ClientMarginSettings table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientMarginSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ClientMarginSettings] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ClientId] INT NOT NULL,
        [TargetMarginPercent] DECIMAL(5,2) NOT NULL,
        [TargetHourlyRate] DECIMAL(18,2) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_ClientMarginSettings] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ClientMarginSettings_Clients_ClientId] FOREIGN KEY ([ClientId]) 
            REFERENCES [dbo].[Clients] ([Id]) ON DELETE CASCADE
    );
    
    -- Create unique index on ClientId
    CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientMarginSettings_ClientId] 
        ON [dbo].[ClientMarginSettings]([ClientId] ASC);
    
    PRINT 'Table ClientMarginSettings created';
END
GO
