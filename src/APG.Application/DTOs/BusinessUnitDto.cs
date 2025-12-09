using System.ComponentModel.DataAnnotations;

namespace APG.Application.DTOs;

/// <summary>
/// DTO for Business Unit entity - returned from API
/// </summary>
public class BusinessUnitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<SectorDto> Sectors { get; set; } = new();
}

/// <summary>
/// DTO for creating or updating a Business Unit
/// </summary>
public class BusinessUnitCreateUpdateDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Manager name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Manager name must be between 2 and 200 characters")]
    public string ManagerName { get; set; } = string.Empty;

    /// <summary>
    /// List of sector IDs to attach to this business unit
    /// </summary>
    public List<int> SectorIds { get; set; } = new();
}
