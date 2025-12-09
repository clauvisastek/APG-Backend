using System.ComponentModel.DataAnnotations;

namespace APG.Application.DTOs;

/// <summary>
/// DTO for Country entity - returned from API
/// </summary>
public class CountryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IsoCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a Country
/// </summary>
public class CountryCreateDto
{
    [Required(ErrorMessage = "Country name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Country name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "ISO code cannot exceed 10 characters")]
    public string? IsoCode { get; set; }
}
