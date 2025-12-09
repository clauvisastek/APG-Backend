using APG.Application.DTOs;
using APG.Application.Services;
using APG.Domain.Entities;
using APG.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace APG.Persistence.Services;

/// <summary>
/// Implementation of global salary settings service with full activation workflow
/// </summary>
public class GlobalSalarySettingsService : IGlobalSalarySettingsService
{
    private readonly AppDbContext _context;

    public GlobalSalarySettingsService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get the currently active global salary settings
    /// </summary>
    public async Task<ActiveGlobalSalarySettingsResponse> GetActiveAsync()
    {
        var activeSettings = await _context.GlobalSalarySettings
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync();

        if (activeSettings == null)
        {
            return new ActiveGlobalSalarySettingsResponse
            {
                HasActiveSettings = false,
                Settings = null
            };
        }

        return new ActiveGlobalSalarySettingsResponse
        {
            HasActiveSettings = true,
            Settings = MapToDto(activeSettings)
        };
    }

    /// <summary>
    /// Get all global salary settings ordered by creation date (newest first)
    /// </summary>
    public async Task<List<GlobalSalarySettingsDto>> GetAllAsync()
    {
        var settings = await _context.GlobalSalarySettings
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return settings.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Create new global salary settings and automatically activate it
    /// Deactivates any previously active settings
    /// </summary>
    public async Task<GlobalSalarySettingsDto> CreateAsync(CreateGlobalSalarySettingsRequest request)
    {
        // Validate input
        ValidateInput(request.EmployerChargesRate, request.IndirectAnnualCosts, request.BillableHoursPerYear);

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Deactivate all currently active settings
                var currentActive = await _context.GlobalSalarySettings
                    .Where(s => s.IsActive)
                    .ToListAsync();

                foreach (var setting in currentActive)
                {
                    setting.IsActive = false;
                    setting.UpdatedAt = DateTime.UtcNow;
                }

                // Create new settings and mark as active
                var newSettings = new GlobalSalarySettings
                {
                    EmployerChargesRate = request.EmployerChargesRate,
                    IndirectAnnualCosts = request.IndirectAnnualCosts,
                    BillableHoursPerYear = request.BillableHoursPerYear,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.GlobalSalarySettings.Add(newSettings);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapToDto(newSettings);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    /// <summary>
    /// Update existing global salary settings
    /// Keeps the current activation state
    /// </summary>
    public async Task<GlobalSalarySettingsDto> UpdateAsync(int id, UpdateGlobalSalarySettingsRequest request)
    {
        var settings = await _context.GlobalSalarySettings.FindAsync(id);
        if (settings == null)
        {
            throw new KeyNotFoundException($"Global salary settings with ID {id} not found");
        }

        // Validate input
        ValidateInput(request.EmployerChargesRate, request.IndirectAnnualCosts, request.BillableHoursPerYear);

        // Update fields
        settings.EmployerChargesRate = request.EmployerChargesRate;
        settings.IndirectAnnualCosts = request.IndirectAnnualCosts;
        settings.BillableHoursPerYear = request.BillableHoursPerYear;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(settings);
    }

    /// <summary>
    /// Activate a specific global salary settings record
    /// Automatically deactivates the currently active record
    /// </summary>
    public async Task<GlobalSalarySettingsDto> ActivateAsync(int id)
    {
        var targetSettings = await _context.GlobalSalarySettings.FindAsync(id);
        if (targetSettings == null)
        {
            throw new KeyNotFoundException($"Global salary settings with ID {id} not found");
        }

        // If already active, return as-is
        if (targetSettings.IsActive)
        {
            return MapToDto(targetSettings);
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Deactivate all currently active settings
                var currentActive = await _context.GlobalSalarySettings
                    .Where(s => s.IsActive)
                    .ToListAsync();

                foreach (var setting in currentActive)
                {
                    setting.IsActive = false;
                    setting.UpdatedAt = DateTime.UtcNow;
                }

                // Activate target settings
                targetSettings.IsActive = true;
                targetSettings.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapToDto(targetSettings);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    /// <summary>
    /// Soft delete a global salary settings record
    /// Can only delete inactive records
    /// Prevents deletion if it's the only record in the system
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var settings = await _context.GlobalSalarySettings.FindAsync(id);
        if (settings == null)
        {
            throw new KeyNotFoundException($"Global salary settings with ID {id} not found");
        }

        // Cannot delete active settings
        if (settings.IsActive)
        {
            throw new InvalidOperationException("Cannot delete an active configuration. Please activate a different configuration first.");
        }

        // Check if this is the only non-deleted record
        var totalCount = await _context.GlobalSalarySettings.CountAsync();
        if (totalCount == 1)
        {
            throw new InvalidOperationException("Cannot delete the only global salary configuration in the system. At least one configuration must exist.");
        }

        // Soft delete: set IsDeleted flag instead of removing
        settings.IsDeleted = true;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Validate input parameters
    /// </summary>
    private void ValidateInput(decimal employerChargesRate, decimal indirectAnnualCosts, int billableHoursPerYear)
    {
        if (employerChargesRate < 0 || employerChargesRate > 200)
        {
            throw new ArgumentException("Employer charges rate must be between 0 and 200 percent");
        }

        if (indirectAnnualCosts < 0)
        {
            throw new ArgumentException("Indirect annual costs cannot be negative");
        }

        if (billableHoursPerYear <= 0 || billableHoursPerYear > 3000)
        {
            throw new ArgumentException("Billable hours per year must be between 1 and 3000");
        }
    }

    /// <summary>
    /// Map entity to DTO
    /// </summary>
    private GlobalSalarySettingsDto MapToDto(GlobalSalarySettings entity)
    {
        return new GlobalSalarySettingsDto
        {
            Id = entity.Id,
            EmployerChargesRate = entity.EmployerChargesRate,
            IndirectAnnualCosts = entity.IndirectAnnualCosts,
            BillableHoursPerYear = entity.BillableHoursPerYear,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt
        };
    }
}
