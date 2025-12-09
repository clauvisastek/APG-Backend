-- Migration: Make client financial fields optional
-- Description: Alters the Clients table to make financial fields nullable
-- Date: 2024-12-04

-- Make financial fields nullable
ALTER TABLE Clients 
ALTER COLUMN DefaultTargetMarginPercent DECIMAL(5, 2) NULL;

ALTER TABLE Clients 
ALTER COLUMN DefaultMinimumMarginPercent DECIMAL(5, 2) NULL;

ALTER TABLE Clients 
ALTER COLUMN DiscountPercent DECIMAL(5, 2) NULL;

ALTER TABLE Clients 
ALTER COLUMN ForcedVacationDaysPerYear INT NULL;

-- Note: Existing data will retain their values
-- New clients created by non-Admin/CFO users will have NULL values for these fields
