-- SQL Script for Testing Client Margins Warning Feature
-- This script helps create test data to verify the warning indicator

-- ============================================================================
-- SECTION 1: Check Existing Clients
-- ============================================================================

-- View all clients with their margin configuration status
SELECT 
    Id,
    Code,
    Name,
    DefaultTargetMarginPercent,
    DefaultMinimumMarginPercent,
    CASE 
        WHEN DefaultTargetMarginPercent IS NOT NULL 
         AND DefaultMinimumMarginPercent IS NOT NULL 
        THEN 'Complete'
        WHEN DefaultTargetMarginPercent IS NULL 
         AND DefaultMinimumMarginPercent IS NULL
        THEN 'Missing Both'
        ELSE 'Partial'
    END AS MarginStatus
FROM Clients
ORDER BY Name;

-- ============================================================================
-- SECTION 2: Create Test Clients (if needed)
-- ============================================================================

-- Example: Insert a client WITH configured margins
-- NOTE: Update BusinessUnitId, SectorId, CountryId, CurrencyId to match your database
/*
INSERT INTO Clients (
    Code, 
    Name, 
    BusinessUnitId, 
    SectorId,
    CountryId,
    CurrencyId,
    DefaultTargetMarginPercent,
    DefaultMinimumMarginPercent,
    DiscountPercent,
    ContactName,
    ContactEmail,
    IsActive,
    CreatedAt
)
VALUES (
    'TEST-COMPLETE',
    'Test Client - Margins Configured',
    1,  -- Update with valid BusinessUnitId
    1,  -- Update with valid SectorId
    1,  -- Update with valid CountryId
    1,  -- Update with valid CurrencyId
    25.0,  -- Target margin 25%
    15.0,  -- Minimum margin 15%
    5.0,   -- Discount 5%
    'John Doe',
    'john.doe@testclient.com',
    1,
    GETDATE()
);
*/

-- Example: Insert a client WITHOUT margins (both NULL)
/*
INSERT INTO Clients (
    Code, 
    Name, 
    BusinessUnitId, 
    SectorId,
    CountryId,
    CurrencyId,
    DefaultTargetMarginPercent,
    DefaultMinimumMarginPercent,
    ContactName,
    ContactEmail,
    IsActive,
    CreatedAt
)
VALUES (
    'TEST-MISSING',
    'Test Client - No Margins',
    1,  -- Update with valid BusinessUnitId
    1,  -- Update with valid SectorId
    1,  -- Update with valid CountryId
    1,  -- Update with valid CurrencyId
    NULL,  -- No target margin
    NULL,  -- No minimum margin
    'Jane Smith',
    'jane.smith@testclient.com',
    1,
    GETDATE()
);
*/

-- Example: Insert a client with PARTIAL margins (only one configured)
/*
INSERT INTO Clients (
    Code, 
    Name, 
    BusinessUnitId, 
    SectorId,
    CountryId,
    CurrencyId,
    DefaultTargetMarginPercent,
    DefaultMinimumMarginPercent,
    ContactName,
    ContactEmail,
    IsActive,
    CreatedAt
)
VALUES (
    'TEST-PARTIAL',
    'Test Client - Partial Margins',
    1,  -- Update with valid BusinessUnitId
    1,  -- Update with valid SectorId
    1,  -- Update with valid CountryId
    1,  -- Update with valid CurrencyId
    20.0,  -- Target margin 20%
    NULL,  -- Missing minimum margin
    'Bob Johnson',
    'bob.johnson@testclient.com',
    1,
    GETDATE()
);
*/

-- ============================================================================
-- SECTION 3: Update Existing Clients for Testing
-- ============================================================================

-- Remove margins from an existing client (for testing warning display)
/*
UPDATE Clients
SET 
    DefaultTargetMarginPercent = NULL,
    DefaultMinimumMarginPercent = NULL,
    UpdatedAt = GETDATE()
WHERE Code = 'YOUR-CLIENT-CODE';
*/

-- Add margins to an existing client (for testing normal display)
/*
UPDATE Clients
SET 
    DefaultTargetMarginPercent = 22.0,
    DefaultMinimumMarginPercent = 12.0,
    UpdatedAt = GETDATE()
WHERE Code = 'YOUR-CLIENT-CODE';
*/

-- ============================================================================
-- SECTION 4: Verification Queries
-- ============================================================================

-- Count clients by margin configuration status
SELECT 
    CASE 
        WHEN DefaultTargetMarginPercent IS NOT NULL 
         AND DefaultMinimumMarginPercent IS NOT NULL 
        THEN 'Complete'
        WHEN DefaultTargetMarginPercent IS NULL 
         AND DefaultMinimumMarginPercent IS NULL
        THEN 'Missing Both'
        ELSE 'Partial'
    END AS MarginStatus,
    COUNT(*) AS ClientCount
FROM Clients
GROUP BY 
    CASE 
        WHEN DefaultTargetMarginPercent IS NOT NULL 
         AND DefaultMinimumMarginPercent IS NOT NULL 
        THEN 'Complete'
        WHEN DefaultTargetMarginPercent IS NULL 
         AND DefaultMinimumMarginPercent IS NULL
        THEN 'Missing Both'
        ELSE 'Partial'
    END;

-- List clients that will show the warning (missing one or both margins)
SELECT 
    Code,
    Name,
    DefaultTargetMarginPercent,
    DefaultMinimumMarginPercent,
    BusinessUnitCode = (SELECT Code FROM BusinessUnits WHERE Id = Clients.BusinessUnitId)
FROM Clients
WHERE DefaultTargetMarginPercent IS NULL
   OR DefaultMinimumMarginPercent IS NULL
ORDER BY Name;

-- List clients that will show normal display (both margins configured)
SELECT 
    Code,
    Name,
    DefaultTargetMarginPercent,
    DefaultMinimumMarginPercent,
    BusinessUnitCode = (SELECT Code FROM BusinessUnits WHERE Id = Clients.BusinessUnitId)
FROM Clients
WHERE DefaultTargetMarginPercent IS NOT NULL
  AND DefaultMinimumMarginPercent IS NOT NULL
ORDER BY Name;

-- ============================================================================
-- SECTION 5: Cleanup (Optional)
-- ============================================================================

-- Delete test clients (if created)
/*
DELETE FROM Clients 
WHERE Code IN ('TEST-COMPLETE', 'TEST-MISSING', 'TEST-PARTIAL');
*/
