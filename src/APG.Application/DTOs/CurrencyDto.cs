using System.ComponentModel.DataAnnotations;

namespace APG.Application.DTOs;

/// <summary>
/// DTO for Currency entity - returned from API
/// </summary>
public class CurrencyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a Currency
/// </summary>
public class CurrencyCreateDto
{
    [Required(ErrorMessage = "Currency name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Currency name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Currency code is required")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Currency code must be between 2 and 10 characters")]
    public string Code { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "Symbol cannot exceed 10 characters")]
    public string? Symbol { get; set; }
}
