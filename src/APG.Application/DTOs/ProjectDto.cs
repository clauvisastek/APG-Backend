using System.ComponentModel.DataAnnotations;

namespace APG.Application.DTOs;

/// <summary>
/// DTO for Project entity - returned from API
/// </summary>
public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    
    // Client info
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientCode { get; set; } = string.Empty;
    
    // Business Unit info
    public int BusinessUnitId { get; set; }
    public string BusinessUnitCode { get; set; } = string.Empty;
    public string BusinessUnitName { get; set; } = string.Empty;
    
    // Project details
    public string Type { get; set; } = string.Empty; // T&M, Forfait, Autre
    public string? ResponsibleName { get; set; }
    public string Currency { get; set; } = string.Empty; // CAD, USD, EUR
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TargetMargin { get; set; }
    public decimal MinMargin { get; set; }
    public string Status { get; set; } = string.Empty; // En construction, Actif, Terminé, En pause
    public string? Notes { get; set; }
    public decimal? YtdRevenue { get; set; }
    
    // Team members and history (optional, parsed from JSON)
    public List<TeamMemberDto>? TeamMembers { get; set; }
    public List<GlobalMarginHistoryDto>? GlobalMarginHistory { get; set; }
    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new Project
/// </summary>
public class ProjectCreateDto
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Project code is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Code must be between 2 and 50 characters")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Client is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Client")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Business Unit is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Business Unit")]
    public int BusinessUnitId { get; set; }

    [Required(ErrorMessage = "Project type is required")]
    [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
    public string Type { get; set; } = string.Empty; // T&M, Forfait, Autre

    [StringLength(200, ErrorMessage = "Responsible name cannot exceed 200 characters")]
    public string? ResponsibleName { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter code")]
    public string Currency { get; set; } = string.Empty; // CAD, USD, EUR

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Target margin is required")]
    [Range(0, 100, ErrorMessage = "Target margin must be between 0 and 100")]
    public decimal TargetMargin { get; set; }

    [Required(ErrorMessage = "Minimum margin is required")]
    [Range(0, 100, ErrorMessage = "Minimum margin must be between 0 and 100")]
    public decimal MinMargin { get; set; }

    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string? Status { get; set; } // Optional, defaults to "En construction"

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "YTD Revenue must be positive")]
    public decimal? YtdRevenue { get; set; }

    // Team members (optional)
    public List<TeamMemberDto>? TeamMembers { get; set; }

    // Global margin history (optional)
    public List<GlobalMarginHistoryDto>? GlobalMarginHistory { get; set; }
}

/// <summary>
/// DTO for updating an existing Project
/// </summary>
public class ProjectUpdateDto
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Project code is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Code must be between 2 and 50 characters")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Client is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Client")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Business Unit is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Business Unit")]
    public int BusinessUnitId { get; set; }

    [Required(ErrorMessage = "Project type is required")]
    [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
    public string Type { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Responsible name cannot exceed 200 characters")]
    public string? ResponsibleName { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter code")]
    public string Currency { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Target margin is required")]
    [Range(0, 100, ErrorMessage = "Target margin must be between 0 and 100")]
    public decimal TargetMargin { get; set; }

    [Required(ErrorMessage = "Minimum margin is required")]
    [Range(0, 100, ErrorMessage = "Minimum margin must be between 0 and 100")]
    public decimal MinMargin { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string Status { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "YTD Revenue must be positive")]
    public decimal? YtdRevenue { get; set; }

    // Team members (optional)
    public List<TeamMemberDto>? TeamMembers { get; set; }

    // Global margin history (optional)
    public List<GlobalMarginHistoryDto>? GlobalMarginHistory { get; set; }
}

/// <summary>
/// DTO for team member
/// </summary>
public class TeamMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal CostRate { get; set; }
    public decimal SellRate { get; set; }
    public decimal GrossMargin { get; set; }
    public decimal NetMargin { get; set; }
}

/// <summary>
/// DTO for global margin history
/// </summary>
public class GlobalMarginHistoryDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
