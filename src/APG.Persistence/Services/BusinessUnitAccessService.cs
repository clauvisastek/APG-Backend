using APG.Application.Services;
using APG.Domain.Entities;
using APG.Persistence.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APG.Persistence.Services;

/// <summary>
/// Service implementation for Business Unit access control based on user roles
/// </summary>
public class BusinessUnitAccessService : IBusinessUnitAccessService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BusinessUnitAccessService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<BusinessUnit>> GetAccessibleBusinessUnitsAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            // DEVELOPMENT MODE: Return all BUs when no authenticated user (for testing without auth)
            return await _context.BusinessUnits
                .Where(bu => bu.IsActive)
                .OrderBy(bu => bu.Code)
                .ToListAsync();
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

        // Check if user has Admin or CFO role - they can see all Business Units
        if (roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                          r.Equals("CFO", StringComparison.OrdinalIgnoreCase)))
        {
            return await _context.BusinessUnits
                .Where(bu => bu.IsActive)
                .OrderBy(bu => bu.Code)
                .ToListAsync();
        }

        // Extract BU-specific roles (e.g., "BU-001", "BU-002")
        var buCodes = roles
            .Where(r => r.StartsWith("BU-", StringComparison.OrdinalIgnoreCase))
            .Select(r => r.ToUpper())
            .ToList();

        if (!buCodes.Any())
        {
            // User has no BU-specific roles and is not Admin/CFO
            return new List<BusinessUnit>();
        }

        // Return only the Business Units matching the user's BU roles
        return await _context.BusinessUnits
            .Where(bu => bu.IsActive && buCodes.Contains(bu.Code.ToUpper()))
            .OrderBy(bu => bu.Code)
            .ToListAsync();
    }

    public async Task<bool> CanAccessBusinessUnitAsync(int businessUnitId)
    {
        var accessibleBUs = await GetAccessibleBusinessUnitsAsync();
        return accessibleBUs.Any(bu => bu.Id == businessUnitId);
    }
}
