using APG.Application.DTOs;

namespace APG.Application.Services;

/// <summary>
/// Service for Country operations
/// </summary>
public interface ICountryService
{
    Task<List<CountryDto>> GetAllActiveAsync();
    Task<CountryDto?> GetByIdAsync(int id);
    Task<CountryDto> CreateAsync(CountryCreateDto dto);
}
