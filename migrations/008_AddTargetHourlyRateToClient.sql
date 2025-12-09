-- Migration: Add TargetHourlyRate to Clients
-- Description: Adds TargetHourlyRate column to the Clients table for CFO configuration
-- Date: 2024-12-05

-- Add TargetHourlyRate column
ALTER TABLE Clients 
ADD TargetHourlyRate DECIMAL(18, 2) NULL;

-- Note: This field is optional and will be NULL for existing clients
-- CFO/Admin users can configure this value along with other financial parameters

