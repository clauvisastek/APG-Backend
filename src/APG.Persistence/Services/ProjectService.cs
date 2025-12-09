using APG.Application.DTOs;
using APG.Application.Services;
using APG.Domain.Entities;
using APG.Persistence.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace APG.Persistence.Services;

/// <summary>
/// Service implementation for Project operations with validation and business rules
/// </summary>
public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;
    private readonly IBusinessUnitAccessService _businessUnitAccessService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProjectService(
        AppDbContext context,
        IBusinessUnitAccessService businessUnitAccessService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _businessUnitAccessService = businessUnitAccessService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<ProjectDto>> GetAllAsync()
    {
        // Get accessible Business Units for current user
        var accessibleBUs = await _businessUnitAccessService.GetAccessibleBusinessUnitsAsync();
        var accessibleBUIds = accessibleBUs.Select(bu => bu.Id).ToList();

        // If user has no accessible BUs, return empty list
        if (!accessibleBUIds.Any())
        {
            return new List<ProjectDto>();
        }

        var projects = await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.BusinessUnit)
            .Where(p => p.IsActive && accessibleBUIds.Contains(p.BusinessUnitId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return projects.Select(MapToDto).ToList();
    }

    public async Task<ProjectDto?> GetByIdAsync(int id)
    {
        var project = await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.BusinessUnit)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (project == null)
            return null;

        // Check if user can access this project's Business Unit
        var canAccess = await _businessUnitAccessService.CanAccessBusinessUnitAsync(project.BusinessUnitId);
        if (!canAccess)
        {
            throw new UnauthorizedAccessException("You do not have access to this project's Business Unit.");
        }

        return MapToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(ProjectCreateDto dto)
    {
        // Validate Business Unit access
        var canAccess = await _businessUnitAccessService.CanAccessBusinessUnitAsync(dto.BusinessUnitId);
        if (!canAccess)
        {
            throw new UnauthorizedAccessException("You do not have access to the selected Business Unit.");
        }

        // Validate Client exists and is active
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == dto.ClientId && c.IsActive);
        if (client == null)
        {
            throw new InvalidOperationException($"Client with ID {dto.ClientId} not found or is inactive.");
        }

        // Validate Business Unit exists
        var businessUnit = await _context.BusinessUnits
            .FirstOrDefaultAsync(bu => bu.Id == dto.BusinessUnitId && bu.IsActive);
        if (businessUnit == null)
        {
            throw new InvalidOperationException($"Business Unit with ID {dto.BusinessUnitId} not found or is inactive.");
        }

        // Validate dates
        if (dto.EndDate <= dto.StartDate)
        {
            throw new InvalidOperationException("End date must be after start date.");
        }

        // Validate margins
        if (dto.MinMargin > dto.TargetMargin)
        {
            throw new InvalidOperationException("Minimum margin cannot be greater than target margin.");
        }

        // Check for duplicate code (case-insensitive)
        var normalizedCode = dto.Code.Trim().ToUpper();
        var existingProject = await _context.Projects
            .FirstOrDefaultAsync(p => p.Code.ToUpper() == normalizedCode && p.IsActive);
        if (existingProject != null)
        {
            throw new InvalidOperationException($"A project with the code '{dto.Code}' already exists.");
        }

        var project = new Project
        {
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim(),
            ClientId = dto.ClientId,
            BusinessUnitId = dto.BusinessUnitId,
            Type = dto.Type,
            ResponsibleName = dto.ResponsibleName?.Trim(),
            Currency = dto.Currency.ToUpper(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TargetMargin = dto.TargetMargin,
            MinMargin = dto.MinMargin,
            Status = dto.Status ?? "En construction",
            Notes = dto.Notes?.Trim(),
            YtdRevenue = dto.YtdRevenue,
            TeamMembersJson = dto.TeamMembers != null ? JsonSerializer.Serialize(dto.TeamMembers) : null,
            GlobalMarginHistoryJson = dto.GlobalMarginHistory != null ? JsonSerializer.Serialize(dto.GlobalMarginHistory) : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Reload with related entities
        await _context.Entry(project)
            .Reference(p => p.Client)
            .LoadAsync();
        await _context.Entry(project)
            .Reference(p => p.BusinessUnit)
            .LoadAsync();

        return MapToDto(project);
    }

    public async Task<ProjectDto?> UpdateAsync(int id, ProjectUpdateDto dto)
    {
        var project = await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.BusinessUnit)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (project == null)
            return null;

        // Check if user can access this project's current Business Unit
        var canAccessCurrent = await _businessUnitAccessService.CanAccessBusinessUnitAsync(project.BusinessUnitId);
        if (!canAccessCurrent)
        {
            throw new UnauthorizedAccessException("You do not have access to this project's Business Unit.");
        }

        // If Business Unit is being changed, validate access to new BU
        if (project.BusinessUnitId != dto.BusinessUnitId)
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

        // Validate Client exists and is active
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == dto.ClientId && c.IsActive);
        if (client == null)
        {
            throw new InvalidOperationException($"Client with ID {dto.ClientId} not found or is inactive.");
        }

        // Validate dates
        if (dto.EndDate <= dto.StartDate)
        {
            throw new InvalidOperationException("End date must be after start date.");
        }

        // Validate margins
        if (dto.MinMargin > dto.TargetMargin)
        {
            throw new InvalidOperationException("Minimum margin cannot be greater than target margin.");
        }

        // Check for duplicate code (case-insensitive) - excluding current project
        var normalizedCode = dto.Code.Trim().ToUpper();
        var existingProject = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id != id && p.Code.ToUpper() == normalizedCode && p.IsActive);
        if (existingProject != null)
        {
            throw new InvalidOperationException($"A project with the code '{dto.Code}' already exists.");
        }

        // Update project
        project.Name = dto.Name.Trim();
        project.Code = dto.Code.Trim();
        project.ClientId = dto.ClientId;
        project.BusinessUnitId = dto.BusinessUnitId;
        project.Type = dto.Type;
        project.ResponsibleName = dto.ResponsibleName?.Trim();
        project.Currency = dto.Currency.ToUpper();
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.TargetMargin = dto.TargetMargin;
        project.MinMargin = dto.MinMargin;
        project.Status = dto.Status;
        project.Notes = dto.Notes?.Trim();
        project.YtdRevenue = dto.YtdRevenue;
        project.TeamMembersJson = dto.TeamMembers != null ? JsonSerializer.Serialize(dto.TeamMembers) : null;
        project.GlobalMarginHistoryJson = dto.GlobalMarginHistory != null ? JsonSerializer.Serialize(dto.GlobalMarginHistory) : null;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload related entities if they changed
        await _context.Entry(project)
            .Reference(p => p.Client)
            .LoadAsync();
        await _context.Entry(project)
            .Reference(p => p.BusinessUnit)
            .LoadAsync();

        return MapToDto(project);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (project == null)
            return false;

        // Check if user can access this project's Business Unit
        var canAccess = await _businessUnitAccessService.CanAccessBusinessUnitAsync(project.BusinessUnitId);
        if (!canAccess)
        {
            throw new UnauthorizedAccessException("You do not have access to this project's Business Unit.");
        }

        // Soft delete by setting IsActive to false
        project.IsActive = false;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private static ProjectDto MapToDto(Project project)
    {
        List<TeamMemberDto>? teamMembers = null;
        if (!string.IsNullOrEmpty(project.TeamMembersJson))
        {
            try
            {
                teamMembers = JsonSerializer.Deserialize<List<TeamMemberDto>>(project.TeamMembersJson);
            }
            catch
            {
                // Ignore deserialization errors
            }
        }

        List<GlobalMarginHistoryDto>? marginHistory = null;
        if (!string.IsNullOrEmpty(project.GlobalMarginHistoryJson))
        {
            try
            {
                marginHistory = JsonSerializer.Deserialize<List<GlobalMarginHistoryDto>>(project.GlobalMarginHistoryJson);
            }
            catch
            {
                // Ignore deserialization errors
            }
        }

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Code = project.Code,
            ClientId = project.ClientId,
            ClientName = project.Client.Name,
            ClientCode = project.Client.Code,
            BusinessUnitId = project.BusinessUnitId,
            BusinessUnitCode = project.BusinessUnit.Code,
            BusinessUnitName = project.BusinessUnit.Name,
            Type = project.Type,
            ResponsibleName = project.ResponsibleName,
            Currency = project.Currency,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            TargetMargin = project.TargetMargin,
            MinMargin = project.MinMargin,
            Status = project.Status,
            Notes = project.Notes,
            YtdRevenue = project.YtdRevenue,
            TeamMembers = teamMembers,
            GlobalMarginHistory = marginHistory,
            IsActive = project.IsActive,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
}
