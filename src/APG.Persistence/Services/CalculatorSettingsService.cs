using APG.Application.DTOs;
using APG.Application.Services;
using APG.Domain.Entities;
using APG.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace APG.Persistence.Services;

/// <summary>
/// Service implementation for calculator settings operations
/// </summary>
public class CalculatorSettingsService : ICalculatorSettingsService
{
    private readonly AppDbContext _context;

    public CalculatorSettingsService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get current global salary settings (latest by Id)
    /// </summary>
    public async Task<GlobalSalarySettingsDto?> GetCurrentGlobalSettingsAsync()
    {
        var settings = await _context.GlobalSalarySettings
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync();

        if (settings == null)
            return null;

        return new GlobalSalarySettingsDto
        {
            Id = settings.Id,
            EmployerChargesRate = settings.EmployerChargesRate,
            IndirectAnnualCosts = settings.IndirectAnnualCosts,
            BillableHoursPerYear = settings.BillableHoursPerYear
        };
    }

    /// <summary>
    /// Create or update global salary settings
    /// Creates a new record each time (audit trail approach)
    /// </summary>
    public async Task<GlobalSalarySettingsDto> UpsertGlobalSettingsAsync(UpdateGlobalSalarySettingsRequest request)
    {
        // Validate request
        if (request.EmployerChargesRate < 0 || request.EmployerChargesRate > 100)
            throw new ArgumentException("Employer charges rate must be between 0 and 100");

        if (request.IndirectAnnualCosts < 0)
            throw new ArgumentException("Indirect annual costs cannot be negative");

        if (request.BillableHoursPerYear <= 0)
            throw new ArgumentException("Billable hours per year must be greater than 0");

        // Create new settings record
        var settings = new GlobalSalarySettings
        {
            EmployerChargesRate = request.EmployerChargesRate,
            IndirectAnnualCosts = request.IndirectAnnualCosts,
            BillableHoursPerYear = request.BillableHoursPerYear,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.GlobalSalarySettings.Add(settings);
        await _context.SaveChangesAsync();

        return new GlobalSalarySettingsDto
        {
            Id = settings.Id,
            EmployerChargesRate = settings.EmployerChargesRate,
            IndirectAnnualCosts = settings.IndirectAnnualCosts,
            BillableHoursPerYear = settings.BillableHoursPerYear
        };
    }

    /// <summary>
    /// Get all client margin settings
    /// </summary>
    public async Task<List<ClientMarginSettingsDto>> GetClientMarginSettingsAsync()
    {
        var settings = await _context.ClientMarginSettings
            .Include(s => s.Client)
            .OrderBy(s => s.Client.Name)
            .ToListAsync();

        return settings.Select(s => new ClientMarginSettingsDto
        {
            Id = s.Id,
            ClientId = s.ClientId,
            ClientName = s.Client.Name,
            TargetMarginPercent = s.TargetMarginPercent,
            TargetHourlyRate = s.TargetHourlyRate
        }).ToList();
    }

    /// <summary>
    /// Create new client margin settings
    /// </summary>
    public async Task<ClientMarginSettingsDto> CreateClientMarginSettingsAsync(CreateClientMarginSettingsRequest request)
    {
        // Validate that client exists
        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null)
            throw new KeyNotFoundException($"Client with ID {request.ClientId} not found");

        // Validate that settings don't already exist for this client
        var existingSettings = await _context.ClientMarginSettings
            .FirstOrDefaultAsync(s => s.ClientId == request.ClientId);

        if (existingSettings != null)
            throw new InvalidOperationException($"Margin settings already exist for client '{client.Name}'. Use update instead.");

        // Validate request
        if (request.TargetMarginPercent < 0 || request.TargetMarginPercent > 100)
            throw new ArgumentException("Target margin percent must be between 0 and 100");

        if (request.TargetHourlyRate < 0)
            throw new ArgumentException("Target hourly rate cannot be negative");

        // Create new settings
        var clientMarginSettings = new ClientMarginSettings
        {
            ClientId = request.ClientId,
            TargetMarginPercent = request.TargetMarginPercent,
            TargetHourlyRate = request.TargetHourlyRate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ClientMarginSettings.Add(clientMarginSettings);
        await _context.SaveChangesAsync();

        // Reload with client data
        await _context.Entry(clientMarginSettings).Reference(s => s.Client).LoadAsync();

        return new ClientMarginSettingsDto
        {
            Id = clientMarginSettings.Id,
            ClientId = clientMarginSettings.ClientId,
            ClientName = clientMarginSettings.Client.Name,
            TargetMarginPercent = clientMarginSettings.TargetMarginPercent,
            TargetHourlyRate = clientMarginSettings.TargetHourlyRate
        };
    }

    /// <summary>
    /// Update existing client margin settings
    /// </summary>
    public async Task<ClientMarginSettingsDto> UpdateClientMarginSettingsAsync(int id, UpdateClientMarginSettingsRequest request)
    {
        var settings = await _context.ClientMarginSettings
            .Include(s => s.Client)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (settings == null)
            throw new KeyNotFoundException($"Client margin settings with ID {id} not found");

        // Validate request
        if (request.TargetMarginPercent < 0 || request.TargetMarginPercent > 100)
            throw new ArgumentException("Target margin percent must be between 0 and 100");

        if (request.TargetHourlyRate < 0)
            throw new ArgumentException("Target hourly rate cannot be negative");

        // Update settings
        settings.TargetMarginPercent = request.TargetMarginPercent;
        settings.TargetHourlyRate = request.TargetHourlyRate;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ClientMarginSettingsDto
        {
            Id = settings.Id,
            ClientId = settings.ClientId,
            ClientName = settings.Client.Name,
            TargetMarginPercent = settings.TargetMarginPercent,
            TargetHourlyRate = settings.TargetHourlyRate
        };
    }

    /// <summary>
    /// Delete client margin settings
    /// </summary>
    public async Task DeleteClientMarginSettingsAsync(int id)
    {
        var settings = await _context.ClientMarginSettings.FindAsync(id);

        if (settings == null)
            throw new KeyNotFoundException($"Client margin settings with ID {id} not found");

        _context.ClientMarginSettings.Remove(settings);
        await _context.SaveChangesAsync();
    }
}
