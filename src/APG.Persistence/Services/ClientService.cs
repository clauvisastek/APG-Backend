using APG.Application.DTOs;
using APG.Application.Services;
using APG.Domain.Entities;
using APG.Persistence.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APG.Persistence.Services;

/// <summary>
/// Service implementation for Client operations with validation and business rules
/// </summary>
public class ClientService : IClientService
{
    private readonly AppDbContext _context;
    private readonly IBusinessUnitAccessService _businessUnitAccessService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientService(
        AppDbContext context, 
        IBusinessUnitAccessService businessUnitAccessService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _businessUnitAccessService = businessUnitAccessService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Checks if the current user can edit financial fields (Admin or CFO roles)
    /// </summary>
    private bool CanEditFinancialFields()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            // DEVELOPMENT MODE: Allow editing when no auth (for testing)
            return true;
        }

        // Extract roles from custom claims (Auth0 uses https://apg-astek.com/roles)
        var roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role || 
                       c.Type == "role" || 
                       c.Type == "roles" ||
                       c.Type == "https://apg-astek.com/roles")
            .SelectMany(c => 
            {
                // Handle both single string and JSON array format
                if (c.Value.StartsWith("["))
                {
                    try
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<string[]>(c.Value) ?? Array.Empty<string>();
                    }
                    catch
                    {
                        return new[] { c.Value };
                    }
                }
                return new[] { c.Value };
            })
            .ToList();

        return roles.Any(r => 
            r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
            r.Equals("CFO", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<ClientDto>> GetAllAsync()
    {
        // Get accessible Business Units for current user
        var accessibleBUs = await _businessUnitAccessService.GetAccessibleBusinessUnitsAsync();
        var accessibleBUIds = accessibleBUs.Select(bu => bu.Id).ToList();

        // If user has no accessible BUs, return empty list
        if (!accessibleBUIds.Any())
        {
            return new List<ClientDto>();
        }

        var clients = await _context.Clients
            .Include(c => c.Sector)
            .Include(c => c.BusinessUnit)
            .Include(c => c.Country)
            .Include(c => c.Currency)
            .Where(c => c.IsActive && accessibleBUIds.Contains(c.BusinessUnitId))
            .OrderBy(c => c.Name)
            .ToListAsync();

        return clients.Select(MapToDto).ToList();
    }

    public async Task<ClientDto?> GetByIdAsync(int id)
    {
        var client = await _context.Clients
            .Include(c => c.BusinessUnit)
            .Include(c => c.Country)
            .Include(c => c.Currency)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
            return null;

        // Check if user can access this client's Business Unit
        var canAccess = await _businessUnitAccessService.CanAccessBusinessUnitAsync(client.BusinessUnitId);
        if (!canAccess)
        {
            throw new UnauthorizedAccessException("You do not have access to this client's Business Unit.");
        }

        return MapToDto(client);
    }

    public async Task<ClientDto> CreateAsync(ClientCreateUpdateDto dto)
    {
        // Validate Business Unit access
        var canAccess = await _businessUnitAccessService.CanAccessBusinessUnitAsync(dto.BusinessUnitId);
        if (!canAccess)
        {
            throw new UnauthorizedAccessException("You do not have access to the selected Business Unit.");
        }

        // Validate Sector exists and is active
        var sector = await _context.Sectors
            .FirstOrDefaultAsync(s => s.Id == dto.SectorId && s.IsActive);
        if (sector == null)
        {
            throw new InvalidOperationException($"Sector with ID {dto.SectorId} not found or is inactive.");
        }

        // Validate Business Unit exists
        var businessUnit = await _context.BusinessUnits
            .FirstOrDefaultAsync(bu => bu.Id == dto.BusinessUnitId && bu.IsActive);
        if (businessUnit == null)
        {
            throw new InvalidOperationException($"Business Unit with ID {dto.BusinessUnitId} not found or is inactive.");
        }

        // Validate Country exists and is active
        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == dto.CountryId && c.IsActive);
        if (country == null)
        {
            throw new InvalidOperationException($"Country with ID {dto.CountryId} not found or is inactive.");
        }

        // Validate Currency exists and is active
        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == dto.CurrencyId && c.IsActive);
        if (currency == null)
        {
            throw new InvalidOperationException($"Currency with ID {dto.CurrencyId} not found or is inactive.");
        }

        // Validate margin percentages if both are provided
        if (dto.DefaultMinimumMarginPercent.HasValue && 
            dto.DefaultTargetMarginPercent.HasValue &&
            dto.DefaultMinimumMarginPercent > dto.DefaultTargetMarginPercent)
        {
            throw new InvalidOperationException("Minimum margin cannot be greater than target margin.");
        }

        // Check for duplicate code (case-insensitive)
        var normalizedCode = dto.Code.Trim().ToUpper();
        var existingClient = await _context.Clients
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == normalizedCode);
        if (existingClient != null)
        {
            throw new InvalidOperationException($"A client with the code '{dto.Code}' already exists.");
        }

        // Check if user can edit financial fields
        var canEditFinancial = CanEditFinancialFields();

        var client = new Client
        {
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            SectorId = dto.SectorId,
            BusinessUnitId = dto.BusinessUnitId,
            CountryId = dto.CountryId,
            CurrencyId = dto.CurrencyId,
            // Only set financial fields if user has permission
            DefaultTargetMarginPercent = canEditFinancial ? dto.DefaultTargetMarginPercent : null,
            DefaultMinimumMarginPercent = canEditFinancial ? dto.DefaultMinimumMarginPercent : null,
            DiscountPercent = canEditFinancial ? dto.DiscountPercent : null,
            ForcedVacationDaysPerYear = canEditFinancial ? dto.ForcedVacationDaysPerYear : null,
            TargetHourlyRate = canEditFinancial ? dto.TargetHourlyRate : null,
            ContactName = dto.ContactName.Trim(),
            ContactEmail = dto.ContactEmail.Trim().ToLower(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Reload with related entities
        await _context.Entry(client)
            .Reference(c => c.Sector)
            .LoadAsync();
        await _context.Entry(client)
            .Reference(c => c.BusinessUnit)
            .LoadAsync();
        await _context.Entry(client)
            .Reference(c => c.Country)
            .LoadAsync();
        await _context.Entry(client)
            .Reference(c => c.Currency)
            .LoadAsync();

        return MapToDto(client);
    }

    public async Task<ClientDto?> UpdateAsync(int id, ClientCreateUpdateDto dto)
    {
        var client = await _context.Clients
            .Include(c => c.BusinessUnit)
            .Include(c => c.Country)
            .Include(c => c.Currency)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
            return null;

        // Check if user can access this client's current Business Unit
        var canAccessCurrent = await _businessUnitAccessService.CanAccessBusinessUnitAsync(client.BusinessUnitId);
        if (!canAccessCurrent)
        {
            throw new UnauthorizedAccessException("You do not have access to this client's Business Unit.");
        }

        // If Business Unit is being changed, validate access to new BU
        if (client.BusinessUnitId != dto.BusinessUnitId)
        {
            var canAccessNew = await _businessUnitAccessService.CanAccessBusinessUnitAsync(dto.BusinessUnitId);
            if (!canAccessNew)
            {
                throw new UnauthorizedAccessException("You do not have access to the new Business Unit.");
            }

            // Validate new Business Unit exists
            var businessUnit = await _context.BusinessUnits
                .FirstOrDefaultAsync(bu => bu.Id == dto.BusinessUnitId && bu.IsActive);
            if (businessUnit == null)
            {
                throw new InvalidOperationException($"Business Unit with ID {dto.BusinessUnitId} not found or is inactive.");
            }
        }

        // Validate Sector exists and is active
        var sector = await _context.Sectors
            .FirstOrDefaultAsync(s => s.Id == dto.SectorId && s.IsActive);
        if (sector == null)
        {
            throw new InvalidOperationException($"Sector with ID {dto.SectorId} not found or is inactive.");
        }

        // Validate Country exists and is active
        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == dto.CountryId && c.IsActive);
        if (country == null)
        {
            throw new InvalidOperationException($"Country with ID {dto.CountryId} not found or is inactive.");
        }

        // Validate Currency exists and is active
        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == dto.CurrencyId && c.IsActive);
        if (currency == null)
        {
            throw new InvalidOperationException($"Currency with ID {dto.CurrencyId} not found or is inactive.");
        }

        // Validate margin percentages if both are provided
        if (dto.DefaultMinimumMarginPercent.HasValue && 
            dto.DefaultTargetMarginPercent.HasValue &&
            dto.DefaultMinimumMarginPercent > dto.DefaultTargetMarginPercent)
        {
            throw new InvalidOperationException("Minimum margin cannot be greater than target margin.");
        }

        // Check for duplicate code (case-insensitive) - excluding current client
        var normalizedCode = dto.Code.Trim().ToUpper();
        var existingClient = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id != id && c.Code.ToUpper() == normalizedCode);
        if (existingClient != null)
        {
            throw new InvalidOperationException($"A client with the code '{dto.Code}' already exists.");
        }

        // Check if user can edit financial fields
        var canEditFinancial = CanEditFinancialFields();

        // Update client
        client.Code = dto.Code.Trim();
        client.Name = dto.Name.Trim();
        client.SectorId = dto.SectorId;
        client.BusinessUnitId = dto.BusinessUnitId;
        client.CountryId = dto.CountryId;
        client.CurrencyId = dto.CurrencyId;
        
        // Only update financial fields if user has permission
        if (canEditFinancial)
        {
            client.DefaultTargetMarginPercent = dto.DefaultTargetMarginPercent;
            client.DefaultMinimumMarginPercent = dto.DefaultMinimumMarginPercent;
            client.DiscountPercent = dto.DiscountPercent;
            client.ForcedVacationDaysPerYear = dto.ForcedVacationDaysPerYear;
            client.TargetHourlyRate = dto.TargetHourlyRate;
        }
        // Otherwise, leave existing financial values unchanged
        
        client.ContactName = dto.ContactName.Trim();
        client.ContactEmail = dto.ContactEmail.Trim().ToLower();
        client.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload related entities if they changed
        await _context.Entry(client)
            .Reference(c => c.Sector)
            .LoadAsync();
        await _context.Entry(client)
            .Reference(c => c.BusinessUnit)
            .LoadAsync();
        await _context.Entry(client)
            .Reference(c => c.Country)
            .LoadAsync();
        await _context.Entry(client)
            .Reference(c => c.Currency)
            .LoadAsync();

        return MapToDto(client);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
            return false;

        // Check if user can access this client's Business Unit
        var canAccess = await _businessUnitAccessService.CanAccessBusinessUnitAsync(client.BusinessUnitId);
        if (!canAccess)
        {
            throw new UnauthorizedAccessException("You do not have access to this client's Business Unit.");
        }

        // Soft delete by setting IsActive to false
        client.IsActive = false;
        client.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private static ClientDto MapToDto(Client client)
    {
        return new ClientDto
        {
            Id = client.Id,
            Code = client.Code,
            Name = client.Name,
            SectorId = client.SectorId,
            SectorName = client.Sector.Name,
            BusinessUnitId = client.BusinessUnitId,
            BusinessUnitCode = client.BusinessUnit.Code,
            BusinessUnitName = client.BusinessUnit.Name,
            CountryId = client.CountryId,
            CountryName = client.Country.Name,
            CurrencyId = client.CurrencyId,
            CurrencyCode = client.Currency.Code,
            DefaultTargetMarginPercent = client.DefaultTargetMarginPercent,
            DefaultMinimumMarginPercent = client.DefaultMinimumMarginPercent,
            DiscountPercent = client.DiscountPercent,
            ForcedVacationDaysPerYear = client.ForcedVacationDaysPerYear,
            TargetHourlyRate = client.TargetHourlyRate,
            ContactName = client.ContactName,
            ContactEmail = client.ContactEmail,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        };
    }
}
