using APG.Application.DTOs;
using APG.Application.Services;
using APG.Domain.Entities;
using APG.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace APG.Persistence.Services;

/// <summary>
/// Service implementation for Currency operations
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly AppDbContext _context;

    public CurrencyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CurrencyDto>> GetAllActiveAsync()
    {
        var currencies = await _context.Currencies
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync();

        return currencies.Select(MapToDto).ToList();
    }

    public async Task<CurrencyDto?> GetByIdAsync(int id)
    {
        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == id);

        return currency == null ? null : MapToDto(currency);
    }

    public async Task<CurrencyDto> CreateAsync(CurrencyCreateDto dto)
    {
        // Normalize code (uppercase)
        var normalizedCode = dto.Code.Trim().ToUpper();

        // Check for duplicate code (case-insensitive)
        var existingCurrency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Code.ToLower() == normalizedCode.ToLower());

        if (existingCurrency != null)
        {
            throw new InvalidOperationException($"A currency with the code '{normalizedCode}' already exists.");
        }

        var currency = new Currency
        {
            Name = dto.Name.Trim(),
            Code = normalizedCode,
            Symbol = dto.Symbol?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Currencies.Add(currency);
        await _context.SaveChangesAsync();

        return MapToDto(currency);
    }

    private static CurrencyDto MapToDto(Currency currency)
    {
        return new CurrencyDto
        {
            Id = currency.Id,
            Name = currency.Name,
            Code = currency.Code,
            Symbol = currency.Symbol,
            IsActive = currency.IsActive,
            CreatedAt = currency.CreatedAt,
            UpdatedAt = currency.UpdatedAt
        };
    }
}
