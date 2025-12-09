using System.ComponentModel.DataAnnotations;

namespace APG.Application.DTOs;

/// <summary>
/// DTO for Client entity - returned from API
/// </summary>
public class ClientDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    // Sector info
    public int SectorId { get; set; }
    public string SectorName { get; set; } = string.Empty;
    
    // Business Unit info
    public int BusinessUnitId { get; set; }
    public string BusinessUnitCode { get; set; } = string.Empty;
    public string BusinessUnitName { get; set; } = string.Empty;
    
    // Country info
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    
    // Currency info
    public int CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    
    // Margin and calculation fields (optional, only for Admin/CFO)
    public decimal? DefaultTargetMarginPercent { get; set; }
    public decimal? DefaultMinimumMarginPercent { get; set; }
    public decimal? DiscountPercent { get; set; }
    public int? ForcedVacationDaysPerYear { get; set; }
    public decimal? TargetHourlyRate { get; set; }
    
    /// <summary>
    /// Indicates if all financial parameters required by CFO are configured
    /// </summary>
    public bool IsFinancialConfigComplete => 
        DefaultTargetMarginPercent.HasValue && 
        DefaultMinimumMarginPercent.HasValue &&
        DiscountPercent.HasValue &&
        ForcedVacationDaysPerYear.HasValue &&
        TargetHourlyRate.HasValue;
    
    /// <summary>
    /// Status message for financial configuration (for UI display)
    /// </summary>
    public string FinancialConfigStatusMessage => IsFinancialConfigComplete
        ? "Complet"
        : "Paramètres incomplets – à compléter par le CFO (marges, remise, jours de vacances forcés et vendant cible)";
    
    // Contact info
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating or updating a Client
/// </summary>
public class ClientCreateUpdateDto
{
    [Required(ErrorMessage = "Client code is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Code must be between 2 and 50 characters")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Client name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sector is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Sector")]
    public int SectorId { get; set; }

    [Required(ErrorMessage = "Business Unit is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Business Unit")]
    public int BusinessUnitId { get; set; }

    [Required(ErrorMessage = "Country is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Country")]
    public int CountryId { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Currency")]
    public int CurrencyId { get; set; }

    // Financial fields - optional, only editable by Admin/CFO
    [Range(0, 100, ErrorMessage = "Target margin must be between 0 and 100")]
    public decimal? DefaultTargetMarginPercent { get; set; }

    [Range(0, 100, ErrorMessage = "Minimum margin must be between 0 and 100")]
    public decimal? DefaultMinimumMarginPercent { get; set; }

    [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
    public decimal? DiscountPercent { get; set; }

    [Range(0, 365, ErrorMessage = "Forced vacation days must be between 0 and 365")]
    public int? ForcedVacationDaysPerYear { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Target hourly rate must be positive")]
    public decimal? TargetHourlyRate { get; set; }

    [Required(ErrorMessage = "Contact name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Contact name must be between 2 and 200 characters")]
    public string ContactName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string ContactEmail { get; set; } = string.Empty;
}
