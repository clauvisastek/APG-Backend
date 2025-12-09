using APG.Application.DTOs;

namespace APG.Application.Services;

/// <summary>
/// Service for Client operations
/// </summary>
public interface IClientService
{
    Task<List<ClientDto>> GetAllAsync();
    Task<ClientDto?> GetByIdAsync(int id);
    Task<ClientDto> CreateAsync(ClientCreateUpdateDto dto);
    Task<ClientDto?> UpdateAsync(int id, ClientCreateUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
