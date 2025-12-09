using APG.Application.DTOs;

namespace APG.Application.Services;

/// <summary>
/// Service for Currency operations
/// </summary>
public interface ICurrencyService
{
    Task<List<CurrencyDto>> GetAllActiveAsync();
    Task<CurrencyDto?> GetByIdAsync(int id);
    Task<CurrencyDto> CreateAsync(CurrencyCreateDto dto);
}
