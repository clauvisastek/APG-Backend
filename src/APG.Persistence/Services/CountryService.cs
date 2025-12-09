using APG.Application.DTOs;
using APG.Application.Services;
using APG.Domain.Entities;
using APG.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace APG.Persistence.Services;

/// <summary>
/// Service implementation for Country operations
/// </summary>
public class CountryService : ICountryService
{
    private readonly AppDbContext _context;

    public CountryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CountryDto>> GetAllActiveAsync()
    {
        var countries = await _context.Countries
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return countries.Select(MapToDto).ToList();
    }

    public async Task<CountryDto?> GetByIdAsync(int id)
    {
        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == id);

        return country == null ? null : MapToDto(country);
    }

    public async Task<CountryDto> CreateAsync(CountryCreateDto dto)
    {
        // Normalize name (trim and title-case first letter of each word)
        var normalizedName = NormalizeName(dto.Name);

        // Check for duplicate name (case-insensitive)
        var existingCountry = await _context.Countries
            .FirstOrDefaultAsync(c => c.Name.ToLower() == normalizedName.ToLower());

        if (existingCountry != null)
        {
            throw new InvalidOperationException($"A country with the name '{normalizedName}' already exists.");
        }

        var country = new Country
        {
            Name = normalizedName,
            IsoCode = dto.IsoCode?.Trim().ToUpper(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        return MapToDto(country);
    }

    private static CountryDto MapToDto(Country country)
    {
        return new CountryDto
        {
            Id = country.Id,
            Name = country.Name,
            IsoCode = country.IsoCode,
            IsActive = country.IsActive,
            CreatedAt = country.CreatedAt,
            UpdatedAt = country.UpdatedAt
        };
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Trim and title-case (first letter uppercase, rest lowercase for each word)
        var words = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var normalizedWords = words.Select(word =>
            char.ToUpper(word[0]) + word.Substring(1).ToLower()
        );
        return string.Join(" ", normalizedWords);
    }
}
