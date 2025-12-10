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

        // Calculate initial global margin and add to history
        var initialMarginHistory = new List<GlobalMarginHistoryDto>();
        if (dto.TeamMembers != null && dto.TeamMembers.Any())
        {
            var avgMargin = dto.TeamMembers.Average(tm => tm.GrossMargin);
            initialMarginHistory.Add(new GlobalMarginHistoryDto
            {
                Label = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                Value = avgMargin
            });
        }

        // Process team members - create/update Resources and ProjectResources
        if (dto.TeamMembers != null && dto.TeamMembers.Any())
        {
            foreach (var teamMemberDto in dto.TeamMembers)
            {
                // Skip if no email provided
                var email = teamMemberDto.Email ?? teamMemberDto.Id;
                if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                    continue;

                // Parse name to get FirstName and LastName
                var nameParts = teamMemberDto.Name.Split(' ', 2);
                var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
                var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

                // Try to find existing resource by email
                var resource = await _context.Resources
                    .FirstOrDefaultAsync(r => r.Email.ToLower() == email.ToLower());

                // If resource doesn't exist, create it
                if (resource == null)
                {
                    resource = new Resource
                    {
                        Email = email.ToLower(),
                        FirstName = firstName,
                        LastName = lastName,
                        Name = teamMemberDto.Name,
                        JobType = teamMemberDto.Role,
                        DailyCostRate = teamMemberDto.CostRate,
                        DailySellRate = teamMemberDto.SellRate,
                        MarginRate = teamMemberDto.GrossMargin,
                        BusinessUnitId = dto.BusinessUnitId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Resources.Add(resource);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Update existing resource rates if they changed
                    resource.DailyCostRate = teamMemberDto.CostRate;
                    resource.DailySellRate = teamMemberDto.SellRate;
                    resource.MarginRate = teamMemberDto.GrossMargin;
                    resource.UpdatedAt = DateTime.UtcNow;
                }

                // Create ProjectResource association
                var projectResource = new ProjectResource
                {
                    ProjectId = project.Id,
                    ResourceId = resource.Id,
                    Role = teamMemberDto.Role,
                    ResourceType = "Employee", // Default, can be enhanced
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    DailyCostRate = teamMemberDto.CostRate,
                    DailySellRate = teamMemberDto.SellRate,
                    GrossMarginAmount = teamMemberDto.GrossMargin,
                    GrossMarginPercent = teamMemberDto.GrossMargin,
                    NetMarginPercent = teamMemberDto.NetMargin,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };

                _context.ProjectResources.Add(projectResource);
            }

            await _context.SaveChangesAsync();
        }

        // Update GlobalMarginHistoryJson with the initial margin
        if (initialMarginHistory.Any())
        {
            project.GlobalMarginHistoryJson = JsonSerializer.Serialize(initialMarginHistory);
            await _context.SaveChangesAsync();
        }

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
        
        // Update margin history if team members changed
        if (dto.TeamMembers != null && dto.TeamMembers.Any())
        {
            var existingHistory = new List<GlobalMarginHistoryDto>();
            if (!string.IsNullOrEmpty(project.GlobalMarginHistoryJson))
            {
                try
                {
                    existingHistory = JsonSerializer.Deserialize<List<GlobalMarginHistoryDto>>(project.GlobalMarginHistoryJson) ?? new List<GlobalMarginHistoryDto>();
                }
                catch
                {
                    existingHistory = new List<GlobalMarginHistoryDto>();
                }
            }

            var newAvgMargin = dto.TeamMembers.Average(tm => tm.GrossMargin);
            var lastMargin = existingHistory.LastOrDefault();
            
            // Only add new point if margin changed significantly (>0.5%)
            if (lastMargin == null || Math.Abs(lastMargin.Value - newAvgMargin) > 0.5m)
            {
                existingHistory.Add(new GlobalMarginHistoryDto
                {
                    Label = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"),
                    Value = newAvgMargin
                });
                project.GlobalMarginHistoryJson = JsonSerializer.Serialize(existingHistory);
            }
        }
        
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
