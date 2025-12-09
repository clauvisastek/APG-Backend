# QUICKSTART - Client Financial Configuration Changes

## What Changed?

All client financial configuration has been moved to the **Clients** screen. The calculator now relies solely on client data instead of having a separate configuration section.

## For CFO/Admin Users

### Configuring Clients

1. Go to **Clients** page
2. Edit an existing client or create a new one
3. You'll see these financial fields (Admin/CFO only):
   - **Marge cible par défaut (%)** - Target margin percentage
   - **Marge minimale par défaut (%)** - Minimum margin percentage
   - **Remise (%)** - Discount percentage
   - **Jours de vacances forcés par an** - Forced vacation days per year
   - **Vendant cible ($/h)** - Target hourly selling rate
4. Fill all 5 fields to make the client available for calculator simulations
5. Save

### Finding Incomplete Clients

On the Clients page:
- Look for the badge: **"Clients à compléter: X"**
- Click **"Clients à compléter"** filter to see only incomplete clients
- Clients with incomplete config show a warning: ⚠ **"Paramètres incomplets"**

### New Column

The Clients table now has a **"Vendant cible ($/h)"** column showing the target hourly rate.

## For All Users (Calculator)

### Using the Calculator

1. Go to **Calculette de marge**
2. Fill in resource details (type, salary/rate, hours)
3. Select a client from the dropdown
4. **If the client has incomplete config:**
   - ⚠ Warning message appears
   - "Calculer la marge" button is disabled
   - Contact your CFO to complete the client configuration
5. **If the client is fully configured:**
   - Button is enabled
   - Calculate normally

### What's Gone

The **"Paramètres par client"** section has been removed from the CFO configuration area on the calculator page. All client configuration is now done on the Clients screen.

## Technical Notes

### Database Migration

Run this SQL on your database:

```sql
ALTER TABLE Clients 
ADD TargetHourlyRate DECIMAL(18, 2) NULL;
```

Or use Entity Framework:
```bash
cd /path/to/APG_Backend
dotnet ef database update --project src/APG.Persistence --startup-project src/APG.API
```

### API Changes

**ClientDto now includes:**
- `targetHourlyRate?: number | null`
- `isFinancialConfigComplete?: boolean`
- `financialConfigStatusMessage?: string`

**All existing endpoints continue to work**, with these new fields added to responses.

## Migration Path

### For Existing Clients

After the update:
1. Existing clients will have `TargetHourlyRate = NULL`
2. They'll show as "incomplete" in the UI
3. CFO must edit each client to add the missing vendant cible value
4. Once all 5 financial fields are filled, the client becomes available for calculations

### No Data Loss

- All existing client data is preserved
- All existing margin configurations remain
- The change is purely additive

## Support

For issues or questions, contact the development team.

---

**Quick Reference:**
- ✅ All financial config on Clients screen
- ✅ Calculator uses client data only
- ✅ 5 required fields for complete configuration
- ✅ Clear warnings for incomplete clients
- ✅ "Vendant cible" column added
- ❌ "Paramètres par client" section removed from calculator
