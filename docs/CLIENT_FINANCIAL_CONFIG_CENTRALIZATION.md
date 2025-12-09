# Client Financial Configuration Centralization - Implementation Summary

**Date:** December 5, 2024  
**Project:** APG – Astek Profit Guard  
**Objective:** Centralize all client financial configuration on the Clients screen, making the calculator rely solely on client data.

---

## Overview

This implementation consolidates all client financial parameters into the Clients screen. The CFO now configures all financial data (margins, discount, vacation days, and target hourly rate) directly on the client record, and the calculator uses only these values.

---

## Backend Changes

### 1. Client Entity Updates (`APG.Domain/Entities/Client.cs`)

**Added Property:**
- `decimal? TargetHourlyRate` - Target hourly selling rate for the client ($/h)

**Added Computed Property:**
- `bool IsFinancialConfigComplete` - Returns true only when ALL financial fields are configured:
  - `DefaultTargetMarginPercent`
  - `DefaultMinimumMarginPercent`
  - `DiscountPercent`
  - `ForcedVacationDaysPerYear`
  - `TargetHourlyRate`

### 2. Database Migration (`migrations/008_AddTargetHourlyRateToClient.sql`)

```sql
ALTER TABLE Clients 
ADD TargetHourlyRate DECIMAL(18, 2) NULL;
```

**To apply this migration:**
- If using Entity Framework: Run `dotnet ef database update` from the `src/APG.Persistence` directory
- If using manual scripts: Execute the SQL file directly on your SQL Server database

### 3. Client DTOs (`APG.Application/DTOs/ClientDto.cs`)

**Updated `ClientDto`:**
- Added `decimal? TargetHourlyRate`
- Added `bool IsFinancialConfigComplete` (computed)
- Added `string FinancialConfigStatusMessage` (computed) - User-friendly status message

**Updated `ClientCreateUpdateDto`:**
- Added `decimal? TargetHourlyRate` with validation `[Range(0, double.MaxValue)]`

### 4. Client Service (`APG.Persistence/Services/ClientService.cs`)

Updated the `ClientService` to include `TargetHourlyRate` in:
- `Create` method
- `Update` method (only if user has CFO/Admin role)
- `MapToDto` method

---

## Frontend Changes

### 1. Type Definitions

**Updated `Client` interface (`src/types/index.ts`):**
```typescript
export interface Client {
  // ... existing fields
  targetHourlyRate?: number; // Vendant cible ($/h)
  isFinancialConfigComplete?: boolean;
  financialConfigStatusMessage?: string;
}
```

**Updated `ClientDto` interface (`src/services/clientsApi.ts`):**
```typescript
export interface ClientDto {
  // ... existing fields
  targetHourlyRate?: number | null;
  isFinancialConfigComplete?: boolean;
  financialConfigStatusMessage?: string;
}
```

### 2. Clients Page (`src/pages/ClientsPage.tsx`)

**Added Column:**
- "Vendant cible ($/h)" column displays the target hourly rate
- Shows "Non défini" if not configured

**Updated "Marges par défaut" Column:**
- Now checks `isFinancialConfigComplete` instead of just margin fields
- Shows comprehensive warning message when incomplete:
  > "Paramètres incomplets – à compléter par le CFO (marges, remise, jours de vacances forcés et vendant cible)"

**Incomplete Clients Filter:**
- Updated to use `isFinancialConfigComplete` property
- Counts and displays clients with incomplete financial configuration

### 3. Client Form Modal (`src/components/ClientFormModal.tsx`)

**Added Field** (visible only to Admin/CFO):
- "Vendant cible ($/h)" input field
- Validates that value must be positive

**Updated Field Names:**
- `defaultTargetMargin` → `defaultTargetMarginPercent`
- `defaultMinMargin` → `defaultMinimumMarginPercent`
- Added `targetHourlyRate`

**Validation:**
- Target hourly rate must be > 0 if provided
- All financial fields remain optional but are validated if filled

### 4. Calculator Page (`src/pages/CalculettePage.tsx`)

**Major Changes:**
- Removed `CalculetteCfoClientConfig` import and usage
- Removed dependency on `CalculetteConfig`
- Now fetches clients directly from `clientsApi.getAll()`

**Client Validation:**
- Checks `isFinancialConfigComplete` before allowing calculation
- Shows error toast if client config is incomplete
- Handles `CLIENT_FINANCIAL_CONFIG_INCOMPLETE` error from backend

**CFO Section Updates:**
- Removed "Paramètres par client" section
- Updated instructions to direct CFO to configure client parameters on the Clients screen
- Kept "Paramètres globaux – Salariés" and "Import de données" sections

### 5. Calculator Form (`src/components/CalculetteForm.tsx`)

**Added Features:**
- Client interface now includes `isFinancialConfigComplete` property
- `canCalculate` computed property validates:
  - All required base fields are filled
  - Selected client has complete financial configuration

**Warning Display:**
- Shows warning box below client dropdown when selected client is incomplete:
  ```
  ⚠ Paramètres incomplets
  Les paramètres financiers de ce client ne sont pas entièrement configurés...
  ```

**Button State:**
- "Calculer la marge" button is disabled when:
  - Required fields are missing
  - Selected client's financial configuration is incomplete

### 6. Removed Components

**Component to be deprecated:**
- `CalculetteCfoClientConfig.tsx` - No longer used, can be removed after confirming no other dependencies

---

## User Workflow

### For CFO / Admin

1. **Configure Client Financial Parameters:**
   - Go to **Clients** page
   - Click "Modifier" on a client or create a new one
   - In the form, fill out all financial fields (visible only to CFO/Admin):
     - Marge cible par défaut (%)
     - Marge minimale par défaut (%)
     - Remise (%)
     - Jours de vacances forcés par an
     - Vendant cible ($/h)
   - Save the client

2. **Monitor Incomplete Clients:**
   - Use the "Clients à compléter" filter to see clients with incomplete configuration
   - The yellow warning badge shows the count

### For All Users (Calculator)

1. **Using the Calculator:**
   - Go to **Calculette de marge** page
   - Fill in resource information (salarié/pigiste, salaire/tarif, heures)
   - Select a client from the dropdown
   - If the client has incomplete financial config:
     - A warning message appears below the dropdown
     - The "Calculer la marge" button is disabled
     - User is instructed to contact CFO
   - If the client is fully configured:
     - Button is enabled
     - Calculation proceeds normally

---

## Data Migration Notes

### For Existing Clients

After applying the database migration:
- All existing clients will have `TargetHourlyRate = NULL`
- Existing clients with partial financial configuration will now show as "incomplete"
- CFO must update clients to add the missing `TargetHourlyRate` value

### Backward Compatibility

- All financial fields remain nullable/optional
- Clients without full configuration can still be created/edited
- Only the calculator functionality requires complete configuration

---

## API Behavior (Future Implementation)

### Calculator Simulation Endpoint

When implementing the backend calculation API (`POST /api/calculator/simulate`), it should:

1. Accept request with `clientId`
2. Fetch the client from database
3. Check `client.IsFinancialConfigComplete`:
   - If `false`, return `400 Bad Request`:
     ```json
     {
       "code": "CLIENT_FINANCIAL_CONFIG_INCOMPLETE",
       "message": "Les paramètres financiers de ce client ne sont pas entièrement configurés par le CFO."
     }
     ```
   - If `true`, proceed with calculation using all financial fields:
     - `DefaultTargetMarginPercent`
     - `DefaultMinimumMarginPercent`
     - `DiscountPercent`
     - `ForcedVacationDaysPerYear`
     - `TargetHourlyRate`

---

## Testing Checklist

### Backend
- [ ] Migration applies successfully to database
- [ ] Client API returns `isFinancialConfigComplete` and `targetHourlyRate`
- [ ] Only Admin/CFO can modify financial fields
- [ ] Validation prevents negative or invalid values

### Frontend - Clients Page
- [ ] "Vendant cible ($/h)" column displays correctly
- [ ] Warning appears for clients with incomplete config
- [ ] "Clients à compléter" filter works
- [ ] CFO can see and edit all financial fields
- [ ] Non-CFO users cannot see financial fields
- [ ] Form validation works for `targetHourlyRate`

### Frontend - Calculator
- [ ] Client dropdown populated from `/api/clients`
- [ ] Warning appears when selecting incomplete client
- [ ] Button is disabled for incomplete clients
- [ ] Calculation works for complete clients
- [ ] Error handling for incomplete clients
- [ ] "Paramètres par client" section is removed from CFO area

---

## Rollback Plan

If issues occur:

1. **Database:** Keep the migration applied; it only adds a nullable column
2. **Backend:** The change is additive and backward-compatible
3. **Frontend:** 
   - Revert to previous version
   - Restore `CalculetteCfoClientConfig` component
   - Update imports in `CalculettePage.tsx`

---

## Next Steps

1. Apply database migration on all environments (dev, staging, prod)
2. Deploy backend changes
3. Deploy frontend changes
4. Notify CFO users to update client configurations
5. Monitor for any issues during the transition period
6. After successful deployment, remove `CalculetteCfoClientConfig.tsx` file
7. Remove old ClientMarginSettings table and related backend code if no longer needed

---

## Questions & Support

For questions about this implementation, contact the development team.
