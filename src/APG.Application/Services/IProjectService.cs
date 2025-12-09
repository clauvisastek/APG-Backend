using APG.Application.DTOs;

namespace APG.Application.Services;

/// <summary>
/// Service for Project operations
/// </summary>
public interface IProjectService
{
    Task<List<ProjectDto>> GetAllAsync();
    Task<ProjectDto?> GetByIdAsync(int id);
    Task<ProjectDto> CreateAsync(ProjectCreateDto dto);
    Task<ProjectDto?> UpdateAsync(int id, ProjectUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
